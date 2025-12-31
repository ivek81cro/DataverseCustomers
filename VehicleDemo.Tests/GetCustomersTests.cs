using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VehicleDemo.Constants;
using VehicleDemo.Exceptions;
using VehicleDemo.Services;
using Xunit;

namespace VehicleDemo.Tests;

public class GetCustomersTests
{
    [Fact]
    public async Task GetCustomersAsync_ReturnsCustomers_AndSetsAuthorizationHeader()
    {
        var token = "test-token";
        var dataverseUrl = "https://org.example.crm.dynamics.com";

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATAVERSE_URL"] = dataverseUrl
            })
            .Build();

        var handler = new StubHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{ \"value\": [" +
                    $"{{ \"{DataverseConstants.CustomerFields.Id}\": \"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\", \"{DataverseConstants.CustomerFields.Name}\": \"Alice\", \"{DataverseConstants.CustomerFields.Email}\": \"alice@example.com\", \"{DataverseConstants.CustomerFields.Address}\": \"123 Main St\" }}," +
                    $"{{ \"{DataverseConstants.CustomerFields.Id}\": \"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb\", \"{DataverseConstants.CustomerFields.Name}\": \"Bob\", \"{DataverseConstants.CustomerFields.Email}\": \"bob@example.com\", \"{DataverseConstants.CustomerFields.Address}\": \"456 Oak Ave\" }}" +
                    "] }",
                    Encoding.UTF8,
                    "application/json")
            }
        };

        var httpClient = new HttpClient(handler);
        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var auth = new Mock<DataverseAuthService>(new HttpClient(), config);
        auth.Setup(a => a.GetTokenAsync()).ReturnsAsync(token);

        var logger = NullLogger<GetCustomers>.Instance;
        var sut = new GetCustomers(httpFactory.Object, auth.Object, config, logger);

        var customers = await sut.GetCustomersAsync();

        customers.Should().HaveCount(2);
        customers[0].Id.Should().Be(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        customers[0].Name.Should().Be("Alice");
        customers[0].Email.Should().Be("alice@example.com");
        customers[0].Address.Should().Be("123 Main St");
        customers[1].Id.Should().Be(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        customers[1].Name.Should().Be("Bob");
        customers[1].Email.Should().Be("bob@example.com");
        customers[1].Address.Should().Be("456 Oak Ave");

        handler.LastRequest.Should().NotBeNull();
        
        var expectedUrl = DataverseUrlBuilder.BuildCustomersUrl(dataverseUrl);
        handler.LastRequest!.RequestUri!.ToString().Should().Be(expectedUrl);

        handler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        handler.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.LastRequest!.Headers.Authorization!.Parameter.Should().Be(token);

        auth.Verify(a => a.GetTokenAsync(), Times.Once);
        httpFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomersAsync_WhenApiReturnsError_ThrowsDataverseApiException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATAVERSE_URL"] = "https://org.example.crm.dynamics.com"
            })
            .Build();

        var handler = new StubHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized"
            }
        };

        var httpClient = new HttpClient(handler);
        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var auth = new Mock<DataverseAuthService>(new HttpClient(), config);
        auth.Setup(a => a.GetTokenAsync()).ReturnsAsync("test-token");

        var logger = NullLogger<GetCustomers>.Instance;
        var sut = new GetCustomers(httpFactory.Object, auth.Object, config, logger);

        var act = async () => await sut.GetCustomersAsync();

        await act.Should().ThrowAsync<DataverseApiException>()
            .WithMessage("*Failed to fetch customers: Unauthorized*");
    }

    [Fact]
    public async Task GetCustomersAsync_WhenInvalidJson_ThrowsDataverseApiException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATAVERSE_URL"] = "https://org.example.crm.dynamics.com"
            })
            .Build();

        var handler = new StubHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json", Encoding.UTF8, "application/json")
            }
        };

        var httpClient = new HttpClient(handler);
        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var auth = new Mock<DataverseAuthService>(new HttpClient(), config);
        auth.Setup(a => a.GetTokenAsync()).ReturnsAsync("test-token");

        var logger = NullLogger<GetCustomers>.Instance;
        var sut = new GetCustomers(httpFactory.Object, auth.Object, config, logger);

        var act = async () => await sut.GetCustomersAsync();

        await act.Should().ThrowAsync<DataverseApiException>()
            .WithMessage("*Invalid response format from Dataverse*");
    }

    [Fact]
    public async Task GetCustomersAsync_WhenNetworkError_ThrowsDataverseApiException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATAVERSE_URL"] = "https://org.example.crm.dynamics.com"
            })
            .Build();

        var handler = new StubHttpMessageHandler
        {
            ThrowException = new HttpRequestException("Network error")
        };

        var httpClient = new HttpClient(handler);
        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var auth = new Mock<DataverseAuthService>(new HttpClient(), config);
        auth.Setup(a => a.GetTokenAsync()).ReturnsAsync("test-token");

        var logger = NullLogger<GetCustomers>.Instance;
        var sut = new GetCustomers(httpFactory.Object, auth.Object, config, logger);

        var act = async () => await sut.GetCustomersAsync();

        var exception = await act.Should().ThrowAsync<DataverseApiException>();
        exception.WithMessage("*Network error occurred*");
        exception.WithInnerException<HttpRequestException>();
    }

    private class StubHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);
        public Exception? ThrowException { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            
            if (ThrowException != null)
            {
                throw ThrowException;
            }
            
            return Task.FromResult(Response);
        }
    }
}
