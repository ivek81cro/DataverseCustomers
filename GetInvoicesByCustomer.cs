using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using VehicleDemo.Exceptions;
using VehicleDemo.Helpers;
using VehicleDemo.Services;

namespace VehicleDemo;

/// <summary>
/// Azure Function to retrieve invoices for a specific customer from Dataverse.
/// </summary>
public class GetInvoicesByCustomer
{
    private readonly InvoiceService _invoiceService;
    private readonly ILogger<GetInvoicesByCustomer> _logger;

    public GetInvoicesByCustomer(
        InvoiceService invoiceService,
        ILogger<GetInvoicesByCustomer> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// HTTP GET endpoint to retrieve invoices for a specific customer.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="customerId">The customer ID from the route parameter.</param>
    /// <returns>HTTP response with list of invoices or error details.</returns>
    [Function("GetInvoicesByCustomer")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId}/invoices")]
        HttpRequestData req,
        string customerId)
    {
        _logger.LogInformation("GET /api/customers/{CustomerId}/invoices", customerId);

        if (!Guid.TryParse(customerId, out var customerGuid))
        {
            return await HttpResponseHelper.CreateBadRequestResponseAsync(req, "Invalid customer ID format");
        }

        try
        {
            var invoices = await _invoiceService.GetInvoicesByCustomerIdAsync(customerGuid);
            return await HttpResponseHelper.CreateSuccessResponseAsync(req, invoices);
        }
        catch (DataverseApiException ex)
        {
            _logger.LogError(ex, "Dataverse API error occurred");
            return await HttpResponseHelper.CreateBadGatewayResponseAsync(req, "Failed to retrieve invoices from Dataverse");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            return await HttpResponseHelper.CreateInternalServerErrorResponseAsync(req, "An unexpected error occurred");
        }
    }
}