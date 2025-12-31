namespace VehicleDemo.Exceptions;

public class DataverseApiException : Exception
{
    public DataverseApiException(string message) : base(message)
    {
    }

    public DataverseApiException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
