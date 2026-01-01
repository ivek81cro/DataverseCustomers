using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VehicleDemo.Configuration;
using VehicleDemo.Constants;
using VehicleDemo.Models;

namespace VehicleDemo.Services;

/// <summary>
/// Service for invoice-related operations with Dataverse.
/// </summary>
public class InvoiceService : DataverseServiceBase
{
    public InvoiceService(
        IHttpClientFactory httpFactory,
        IDataverseAuthService authService,
        IOptions<DataverseOptions> options,
        ILogger<InvoiceService> logger)
        : base(httpFactory, authService, options, logger)
    {
    }

    /// <summary>
    /// Retrieves all active invoices with customer information from Dataverse.
    /// </summary>
    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync()
    {
        var url = DataverseUrlBuilder.BuildInvoicesUrl(Options.DataverseUrl);
        var json = await ExecuteDataverseRequestAsync(url);
        return ParseDataverseResponse(json, MapInvoice, "invoices");
    }

    /// <summary>
    /// Retrieves invoices for a specific customer from Dataverse.
    /// </summary>
    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesByCustomerIdAsync(Guid customerId)
    {
        var url = DataverseUrlBuilder.BuildInvoicesByCustomerUrl(Options.DataverseUrl, customerId);
        var json = await ExecuteDataverseRequestAsync(url);
        return ParseDataverseResponse(json, x => MapInvoiceWithCustomerId(x, customerId), "invoices");
    }

    private static InvoiceDto MapInvoice(JsonElement element)
    {
        var customerId = GetGuidValue(element, DataverseConstants.InvoiceFields.CustomerId);
        return MapInvoiceWithCustomerId(element, customerId);
    }

    private static InvoiceDto MapInvoiceWithCustomerId(JsonElement element, Guid customerId)
    {
        return new InvoiceDto(
            Guid.Parse(element.GetProperty(DataverseConstants.InvoiceFields.Id).GetString()!),
            GetStringValue(element, DataverseConstants.InvoiceFields.InvoiceNumber),
            element.GetProperty(DataverseConstants.InvoiceFields.TotalAmount).GetDecimal(),
            GetStringValue(element, DataverseConstants.InvoiceFields.Status),
            customerId,
            GetStringValue(element, DataverseConstants.InvoiceFields.CustomerName)
        );
    }
}
