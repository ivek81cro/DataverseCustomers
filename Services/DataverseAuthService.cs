using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VehicleDemo.Configuration;

namespace VehicleDemo.Services;

/// <summary>
/// Service for handling OAuth 2.0 authentication with Microsoft Dataverse.
/// Implements token caching to minimize authentication requests.
/// </summary>
public class DataverseAuthService : IDataverseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly DataverseOptions _options;
    private readonly ILogger<DataverseAuthService> _logger;
    
    private string? _cachedToken;
    private DateTime _tokenExpiryTime;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public DataverseAuthService(
        HttpClient httpClient,
        IOptions<DataverseOptions> options,
        ILogger<DataverseAuthService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GetTokenAsync()
    {
        // Check if cached token is still valid
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiryTime)
        {
            _logger.LogDebug("Using cached Dataverse access token");
            return _cachedToken;
        }

        await _tokenLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiryTime)
            {
                return _cachedToken;
            }

            _logger.LogInformation("Requesting new Dataverse access token");

            var body = new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["grant_type"] = "client_credentials",
                ["scope"] = _options.GetScopeUrl()
            };

            var response = await _httpClient.PostAsync(
                _options.GetTokenUrl(),
                new FormUrlEncodedContent(body));

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            _cachedToken = doc.RootElement.GetProperty("access_token").GetString()!;
            _tokenExpiryTime = DateTime.UtcNow.AddMinutes(_options.TokenCacheDurationMinutes);

            _logger.LogInformation("Successfully obtained new Dataverse access token. Expires at: {ExpiryTime}", 
                _tokenExpiryTime);

            return _cachedToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to obtain Dataverse access token due to network error");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse token response from authentication service");
            throw;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    /// <inheritdoc/>
    public void ClearTokenCache()
    {
        _logger.LogInformation("Clearing cached Dataverse access token");
        _cachedToken = null;
        _tokenExpiryTime = DateTime.MinValue;
    }
}
