using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using VehicleDemo.Exceptions;
using VehicleDemo.Helpers;
using VehicleDemo.Services;

namespace VehicleDemo;

/// <summary>
/// Azure Function to retrieve all invoices with customer information from Dataverse.
/// </summary>
public class GetInvoices
{
    private readonly InvoiceService _invoiceService;
    private readonly ILogger<GetInvoices> _logger;

    public GetInvoices(
        InvoiceService invoiceService,
        ILogger<GetInvoices> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    [Function("GetInvoices")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoices")]
        HttpRequestData req)
    {
        _logger.LogInformation("GET /api/invoices");

        try
        {
            var invoices = await _invoiceService.GetInvoicesAsync();
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