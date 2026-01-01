using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace VehicleDemo.Helpers;

/// <summary>
/// Helper class for creating standardized HTTP responses in Azure Functions.
/// </summary>
public static class HttpResponseHelper
{
    /// <summary>
    /// Creates a successful HTTP response with JSON body.
    /// </summary>
    public static async Task<HttpResponseData> CreateSuccessResponseAsync(
        HttpRequestData request,
        object data,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(data);
        return response;
    }

    /// <summary>
    /// Creates an error HTTP response with error message.
    /// </summary>
    public static async Task<HttpResponseData> CreateErrorResponseAsync(
        HttpRequestData request,
        HttpStatusCode statusCode,
        string errorMessage)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new { error = errorMessage });
        return response;
    }

    /// <summary>
    /// Creates a 400 Bad Request response.
    /// </summary>
    public static Task<HttpResponseData> CreateBadRequestResponseAsync(
        HttpRequestData request,
        string errorMessage)
        => CreateErrorResponseAsync(request, HttpStatusCode.BadRequest, errorMessage);

    /// <summary>
    /// Creates a 502 Bad Gateway response.
    /// </summary>
    public static Task<HttpResponseData> CreateBadGatewayResponseAsync(
        HttpRequestData request,
        string errorMessage)
        => CreateErrorResponseAsync(request, HttpStatusCode.BadGateway, errorMessage);

    /// <summary>
    /// Creates a 500 Internal Server Error response.
    /// </summary>
    public static Task<HttpResponseData> CreateInternalServerErrorResponseAsync(
        HttpRequestData request,
        string errorMessage)
        => CreateErrorResponseAsync(request, HttpStatusCode.InternalServerError, errorMessage);
}
