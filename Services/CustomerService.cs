using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VehicleDemo.Configuration;
using VehicleDemo.Constants;
using VehicleDemo.Models;

namespace VehicleDemo.Services;

/// <summary>
/// Service for customer-related operations with Dataverse.
/// </summary>
public class CustomerService : DataverseServiceBase
{
    public CustomerService(
        IHttpClientFactory httpFactory,
        IDataverseAuthService authService,
        IOptions<DataverseOptions> options,
        ILogger<CustomerService> logger)
        : base(httpFactory, authService, options, logger)
    {
    }

    /// <summary>
    /// Retrieves all active customers from Dataverse.
    /// </summary>
    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync()
    {
        var url = DataverseUrlBuilder.BuildCustomersUrl(Options.DataverseUrl);
        var json = await ExecuteDataverseRequestAsync(url);
        return ParseDataverseResponse(json, MapCustomer, "customers");
    }

    private static CustomerDto MapCustomer(JsonElement element)
    {
        return new CustomerDto(
            Guid.Parse(element.GetProperty(DataverseConstants.CustomerFields.Id).GetString()!),
            element.GetProperty(DataverseConstants.CustomerFields.Name).GetString()!,
            element.GetProperty(DataverseConstants.CustomerFields.Address).GetString()!,
            element.GetProperty(DataverseConstants.CustomerFields.Email).GetString()!
        );
    }
}
