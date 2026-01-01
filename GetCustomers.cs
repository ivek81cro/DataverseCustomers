using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using VehicleDemo.Exceptions;
using VehicleDemo.Helpers;
using VehicleDemo.Models;
using VehicleDemo.Services;

namespace VehicleDemo;

/// <summary>
/// Azure Function to retrieve all customers from Dataverse.
/// </summary>
public class GetCustomers
{
    private readonly CustomerService _customerService;
    private readonly ILogger<GetCustomers> _logger;

    public GetCustomers(
        CustomerService customerService,
        ILogger<GetCustomers> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active customers from Dataverse
    /// </summary>
    /// <param name="req">HTTP request data</param>
    /// <returns>List of customers or error response</returns>
    /// <response code="200">Returns the list of active customers</response>
    /// <response code="502">Dataverse API error occurred</response>
    /// <response code="500">Internal server error occurred</response>
    [Function("GetCustomers")]
    [OpenApiOperation(operationId: "GetCustomers", tags: new[] { "Customers" }, Summary = "Get all customers", Description = "Retrieves all active customers from Microsoft Dataverse. Returns customer information including ID, name, address, and email.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<CustomerDto>), Description = "Successfully retrieved the list of customers")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadGateway, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Dataverse API error - network error, invalid response, or authentication failure")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error - unexpected error occurred")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")]
        HttpRequestData req)
    {
        _logger.LogInformation("GET /api/customers");

        try
        {
            var customers = await _customerService.GetCustomersAsync();
            return await HttpResponseHelper.CreateSuccessResponseAsync(req, customers);
        }
        catch (DataverseApiException ex)
        {
            _logger.LogError(ex, "Dataverse API error occurred");
            return await HttpResponseHelper.CreateBadGatewayResponseAsync(req, "Failed to retrieve customers from Dataverse");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            return await HttpResponseHelper.CreateInternalServerErrorResponseAsync(req, "An unexpected error occurred");
        }
    }
}