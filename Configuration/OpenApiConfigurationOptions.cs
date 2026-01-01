using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace VehicleDemo.Configuration;

/// <summary>
/// OpenAPI configuration for Swagger documentation
/// </summary>
public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Version = "v1",
        Title = "Demo - Dataverse Customer & Invoice API",
        Description = "Azure Functions application for retrieving customers and invoices from Microsoft Dataverse with relational data support. Implements modern .NET 10 best practices with comprehensive error handling, structured logging, and extensive test coverage.",
        Contact = new OpenApiContact
        {
            Url = new Uri("https://github.com/ivek81cro/DataverseCustomers")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}
