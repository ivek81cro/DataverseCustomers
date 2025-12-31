using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VehicleDemo.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddTransient<DataverseAuthService>();
    })
    .Build();

host.Run();
