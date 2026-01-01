using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using VehicleDemo.Exceptions;
using VehicleDemo.Helpers;
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

    [Function("GetCustomers")]
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