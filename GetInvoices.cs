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

    /// <summary>
    /// Retrieves all active invoices with related customer information
    /// </summary>
    /// <param name="req">HTTP request data</param>
    /// <returns>List of invoices with customer details or error response</returns>
    /// <response code="200">Returns the list of invoices with customer information</response>
    /// <response code="502">Dataverse API error occurred</response>
    /// <response code="500">Internal server error occurred</response>
    [Function("GetInvoices")]
    [OpenApiOperation(operationId: "GetInvoices", tags: new[] { "Invoices" }, Summary = "Get all invoices", Description = "Retrieves all active invoices from Microsoft Dataverse with related customer information using OData $expand. Returns invoice details including ID, invoice number, total amount, status, and customer data.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<InvoiceDto>), Description = "Successfully retrieved the list of invoices with customer information")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadGateway, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Dataverse API error - network error, invalid response, or authentication failure")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error - unexpected error occurred")]
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