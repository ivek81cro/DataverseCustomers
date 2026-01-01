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

    public static string BuildInvoicesUrl(string baseUrl)
    {
        var fields = string.Join(",",
            DataverseConstants.InvoiceFields.Id,
            DataverseConstants.InvoiceFields.InvoiceNumber,
            DataverseConstants.InvoiceFields.TotalAmount,
            DataverseConstants.InvoiceFields.Status,
            DataverseConstants.InvoiceFields.CustomerId);

        return $"{baseUrl}/api/data/{DataverseConstants.ApiVersion}/{DataverseConstants.InvoicesEntity}" +
               $"?$select={fields}" +
               $"&$filter={DataverseConstants.Filters.ActiveRecords}";
    }

    public static string BuildInvoicesByCustomerUrl(string baseUrl, Guid customerId)
    {
        var fields = string.Join(",",
            DataverseConstants.InvoiceFields.Id,
            DataverseConstants.InvoiceFields.InvoiceNumber,
            DataverseConstants.InvoiceFields.TotalAmount,
            DataverseConstants.InvoiceFields.Status,
            DataverseConstants.InvoiceFields.CustomerId);

        // Selecting CustomerId (_cr720_customer_value) automatically includes the formatted value annotation
        return $"{baseUrl}/api/data/{DataverseConstants.ApiVersion}/{DataverseConstants.InvoicesEntity}" +
               $"?$select={fields}" +
               $"&$filter={DataverseConstants.Filters.ActiveRecords} and {DataverseConstants.InvoiceFields.CustomerId} eq {customerId}";
    }
}
