namespace VehicleDemo.Constants;

public static class DataverseConstants
{
    public const string ApiVersion = "v9.2";
    public const string CustomersEntity = "cr720_customers";
    public const string InvoicesEntity = "cr720_invoices";
    
    public static class CustomerFields
    {
        public const string Id = "cr720_customerid";
        public const string Name = "cr720_customername";
        public const string Address = "cr720_address";
        public const string Email = "cr720_email";
    }
    
    public static class InvoiceFields
    {
        public const string Id = "cr720_invoiceid";
        public const string InvoiceNumber = "cr720_invoicenumber";
        public const string TotalAmount = "cr720_totalamount";
        public const string Status = "cr720_status";
        public const string CustomerId = "_cr720_customer_value";
        public const string CustomerName = "_cr720_customer_value@OData.Community.Display.V1.FormattedValue";
    }
    
    public static class Filters
    {
        public const string ActiveRecords = "statecode eq 0";
    }
}
