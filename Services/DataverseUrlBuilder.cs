using VehicleDemo.Constants;

namespace VehicleDemo.Services;

public static class DataverseUrlBuilder
{
    public static string BuildCustomersUrl(string baseUrl)
    {
        var fields = string.Join(",",
            DataverseConstants.CustomerFields.Id,
            DataverseConstants.CustomerFields.Name,
            DataverseConstants.CustomerFields.Address,
            DataverseConstants.CustomerFields.Email);

        return $"{baseUrl}/api/data/{DataverseConstants.ApiVersion}/{DataverseConstants.CustomersEntity}" +
               $"?$select={fields}" +
               $"&$filter={DataverseConstants.Filters.ActiveRecords}";
    }
}
