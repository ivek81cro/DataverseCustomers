using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace VehicleDemo.Services
{
    public class DataverseAuthService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public DataverseAuthService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public virtual async Task<string> GetTokenAsync()
        {
            var tenantId = _config["TENANT_ID"];
            var clientId = _config["CLIENT_ID"];
            var clientSecret = _config["CLIENT_SECRET"];
            var dataverseUrl = _config["DATAVERSE_URL"];

            var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var body = new Dictionary<string, string>
            {
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["grant_type"] = "client_credentials",
                ["scope"] = $"{dataverseUrl}/.default"
            };

            var response = await _http.PostAsync(url, new FormUrlEncodedContent(body));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("access_token").GetString()!;
        }
    }
}
