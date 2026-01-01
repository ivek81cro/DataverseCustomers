namespace VehicleDemo.Models;

/// <summary>
/// Standard error response model for API errors
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error message describing what went wrong
    /// </summary>
    public string Error { get; set; } = string.Empty;

    public ErrorResponse() { }

    public ErrorResponse(string error)
    {
        Error = error;
    }
}
