using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using VehicleDemo.Configuration;
using VehicleDemo.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        // Configure Dataverse options from configuration
        services.Configure<DataverseOptions>(options =>
        {
            var config = context.Configuration;
            options.DataverseUrl = config["DATAVERSE_URL"] ?? string.Empty;
            options.TenantId = config["TENANT_ID"] ?? string.Empty;
            options.ClientId = config["CLIENT_ID"] ?? string.Empty;
            options.ClientSecret = config["CLIENT_SECRET"] ?? string.Empty;
            
            // Optional settings with defaults
            if (int.TryParse(config["TOKEN_CACHE_DURATION_MINUTES"], out var cacheDuration))
            {
                options.TokenCacheDurationMinutes = cacheDuration;
            }
            
            if (int.TryParse(config["REQUEST_TIMEOUT_SECONDS"], out var timeout))
            {
                options.RequestTimeoutSeconds = timeout;
            }
            
            if (int.TryParse(config["MAX_RETRY_ATTEMPTS"], out var retries))
            {
                options.MaxRetryAttempts = retries;
            }
        });

        // Register HTTP clients
        services.AddHttpClient<IDataverseAuthService, DataverseAuthService>();
        services.AddHttpClient();

        // Register services
        services.AddSingleton<IDataverseAuthService, DataverseAuthService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<InvoiceService>();
        
        // Legacy service for backward compatibility with tests
        services.AddTransient<DataverseAuthService>();

        // Configure Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
