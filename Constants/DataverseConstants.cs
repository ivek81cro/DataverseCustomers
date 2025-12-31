namespace VehicleDemo.Constants;

public static class DataverseConstants
{
    public const string ApiVersion = "v9.2";
    public const string CustomersEntity = "cr720_customers";
    
    public static class CustomerFields
    {
        public const string Id = "cr720_customerid";
        public const string Name = "cr720_customername";
        public const string Address = "cr720_address";
        public const string Email = "cr720_email";
    }
    
    public static class Filters
    {
        public const string ActiveRecords = "statecode eq 0";
    }
}
