using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using VehicleDemo.Constants;
using VehicleDemo.Exceptions;
using VehicleDemo.Models;
using VehicleDemo.Services;

namespace VehicleDemo;

public class GetCustomers
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly DataverseAuthService _auth;
    private readonly IConfiguration _config;
    private readonly ILogger<GetCustomers> _logger;

    public GetCustomers(
        IHttpClientFactory httpFactory,
        DataverseAuthService auth,
        IConfiguration config,
        ILogger<GetCustomers> logger)
    {
        _httpFactory = httpFactory;
        _auth = auth;
        _config = config;
        _logger = logger;
    }

    [Function("GetCustomers")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")]
        HttpRequestData req)
    {
        _logger.LogInformation("GET /api/customers");

        try
        {
            var customers = await GetCustomersAsync();

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(customers);

            return res;
        }
        catch (DataverseApiException ex)
        {
            _logger.LogError(ex, "Dataverse API error occurred");
            var errorResponse = req.CreateResponse(HttpStatusCode.BadGateway);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to retrieve customers from Dataverse" });
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
            return errorResponse;
        }
    }

    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync()
    {
        var token = await _auth.GetTokenAsync();
        var client = _httpFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dataverseUrl = _config["DATAVERSE_URL"];
        var url = DataverseUrlBuilder.BuildCustomersUrl(dataverseUrl!);

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while calling Dataverse API at {Url}", url);
            throw new DataverseApiException("Network error occurred while fetching customers", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Dataverse API returned {StatusCode}: {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            throw new DataverseApiException($"Failed to fetch customers: {response.StatusCode} - {response.ReasonPhrase}");
        }

        string json;
        try
        {
            json = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read response content from Dataverse API");
            throw new DataverseApiException("Failed to read API response", ex);
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Dataverse response. JSON: {Json}", json);
            throw new DataverseApiException("Invalid response format from Dataverse", ex);
        }

        using (doc)
        {
            try
            {
                var customers = doc.RootElement
                    .GetProperty("value")
                    .EnumerateArray()
                    .Select(x => new CustomerDto(
                        Guid.Parse(x.GetProperty(DataverseConstants.CustomerFields.Id).GetString()!),
                        x.GetProperty(DataverseConstants.CustomerFields.Name).GetString()!,
                        x.GetProperty(DataverseConstants.CustomerFields.Address).GetString()!,
                        x.GetProperty(DataverseConstants.CustomerFields.Email).GetString()!
                    ))
                    .ToList();

                _logger.LogInformation("Successfully retrieved {Count} customers from Dataverse", customers.Count);
                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to map Dataverse response to CustomerDto");
                throw new DataverseApiException("Failed to process customer data", ex);
            }
        }
    }
}