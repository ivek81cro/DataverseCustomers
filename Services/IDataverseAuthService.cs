namespace VehicleDemo.Services;

/// <summary>
/// Interface for Dataverse authentication service.
/// </summary>
public interface IDataverseAuthService
{
    /// <summary>
    /// Gets an access token for Dataverse API authentication.
    /// Implements token caching to avoid unnecessary authentication requests.
    /// </summary>
    /// <returns>Bearer token string.</returns>
    Task<string> GetTokenAsync();

    /// <summary>
    /// Clears the cached token, forcing re-authentication on next request.
    /// </summary>
    void ClearTokenCache();
}
