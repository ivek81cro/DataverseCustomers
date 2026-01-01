using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using VehicleDemo.Configuration;
using VehicleDemo.Constants;
using VehicleDemo.Exceptions;
using VehicleDemo.Models;

namespace VehicleDemo.Services;

/// <summary>
/// Base service for Dataverse API operations.
/// Provides common functionality for HTTP communication, authentication, and JSON parsing.
/// </summary>
public class DataverseServiceBase
{
    protected readonly IHttpClientFactory HttpFactory;
    protected readonly IDataverseAuthService AuthService;
    protected readonly DataverseOptions Options;
    protected readonly ILogger Logger;

    public DataverseServiceBase(
        IHttpClientFactory httpFactory,
        IDataverseAuthService authService,
        IOptions<DataverseOptions> options,
        ILogger logger)
    {
        HttpFactory = httpFactory;
        AuthService = authService;
        Options = options.Value;
        Logger = logger;
    }

    /// <summary>
    /// Executes an HTTP GET request to Dataverse with authentication and error handling.
    /// </summary>
    protected async Task<string> ExecuteDataverseRequestAsync(string url)
    {
        var token = await AuthService.GetTokenAsync();
        var client = HttpFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Add Prefer header to include OData formatted values
        client.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");

        HttpResponseMessage response;
        try
        {
            Logger.LogDebug("Executing Dataverse GET request: {Url}", url);
            response = await client.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error while calling Dataverse API at {Url}", url);
            throw new DataverseApiException("Network error occurred while communicating with Dataverse", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Logger.LogError("Dataverse API returned {StatusCode}: {Reason}. Details: {ErrorContent}",
                response.StatusCode, response.ReasonPhrase, errorContent);
            throw new DataverseApiException($"Dataverse API request failed: {response.StatusCode} - {response.ReasonPhrase}");
        }

        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read response content from Dataverse API");
            throw new DataverseApiException("Failed to read API response", ex);
        }
    }

    /// <summary>
    /// Parses JSON response and maps to a list of DTOs.
    /// </summary>
    protected IReadOnlyList<T> ParseDataverseResponse<T>(string json, Func<JsonElement, T> mapper, string entityName)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Failed to parse Dataverse response. JSON: {Json}", json);
            throw new DataverseApiException("Invalid JSON response from Dataverse", ex);
        }

        using (doc)
        {
            try
            {
                var items = doc.RootElement
                    .GetProperty("value")
                    .EnumerateArray()
                    .Select(mapper)
                    .ToList();

                Logger.LogInformation("Successfully parsed {Count} {EntityName} from Dataverse response", 
                    items.Count, entityName);

                return items;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to map Dataverse response to {EntityName}", entityName);
                throw new DataverseApiException($"Failed to process {entityName} data", ex);
            }
        }
    }

    /// <summary>
    /// Safely extracts a string value from a JSON element, handling mixed types.
    /// </summary>
    protected static string GetStringValue(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString() ?? string.Empty,
            JsonValueKind.Number => property.GetInt32().ToString(),
            JsonValueKind.Null => string.Empty,
            _ => property.ToString()
        };
    }

    /// <summary>
    /// Safely extracts a GUID value from a JSON element.
    /// </summary>
    protected static Guid GetGuidValue(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return Guid.Empty;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var stringValue = property.GetString();
            return Guid.TryParse(stringValue, out var guid) ? guid : Guid.Empty;
        }

        return Guid.Empty;
    }
}
