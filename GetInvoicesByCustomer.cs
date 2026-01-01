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
    /// Retrieves all invoices for a specific customer
    /// </summary>
    /// <param name="req">HTTP request data</param>
    /// <param name="customerId">The customer ID from the route parameter (GUID format)</param>
    /// <returns>List of invoices for the specified customer or error response</returns>
    /// <response code="200">Returns the list of invoices for the customer</response>
    /// <response code="400">Invalid customer ID format - must be a valid GUID</response>
    /// <response code="502">Dataverse API error occurred</response>
    /// <response code="500">Internal server error occurred</response>
    [Function("GetInvoicesByCustomer")]
    [OpenApiOperation(operationId: "GetInvoicesByCustomer", tags: new[] { "Invoices" }, Summary = "Get invoices by customer", Description = "Retrieves all active invoices for a specific customer from Microsoft Dataverse. Requires a valid customer GUID. Returns invoice details filtered by the specified customer ID.")]
    [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The unique customer identifier (GUID format)")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<InvoiceDto>), Description = "Successfully retrieved the list of invoices for the specified customer")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Invalid customer ID format - the provided customer ID is not a valid GUID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadGateway, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Dataverse API error - network error, invalid response, or authentication failure")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error - unexpected error occurred")]
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