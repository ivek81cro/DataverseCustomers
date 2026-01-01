using System.ComponentModel.DataAnnotations;

namespace VehicleDemo.Configuration;

/// <summary>
/// Configuration options for Dataverse connection and authentication.
/// </summary>
public class DataverseOptions
{
    public const string SectionName = "Dataverse";

    [Required(ErrorMessage = "DATAVERSE_URL is required")]
    public string DataverseUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "TENANT_ID is required")]
    public string TenantId { get; set; } = string.Empty;

    [Required(ErrorMessage = "CLIENT_ID is required")]
    public string ClientId { get; set; } = string.Empty;

    [Required(ErrorMessage = "CLIENT_SECRET is required")]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Token cache duration in minutes. Default is 50 minutes (tokens typically expire after 60 minutes).
    /// </summary>
    public int TokenCacheDurationMinutes { get; set; } = 50;

    /// <summary>
    /// HTTP request timeout in seconds. Default is 30 seconds.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for transient failures. Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    public string GetTokenUrl() => $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";

    public string GetScopeUrl() => $"{DataverseUrl}/.default";
}
