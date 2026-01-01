using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using VehicleDemo.Configuration;
using VehicleDemo.Exceptions;
using VehicleDemo.Services;
using Xunit;

namespace VehicleDemo.Tests;

public class GetInvoicesTests
{
    private readonly Mock<IHttpClientFactory> _httpFactoryMock;
    private readonly Mock<IDataverseAuthService> _authMock;
    private readonly IOptions<DataverseOptions> _options;
    private readonly Mock<ILogger<InvoiceService>> _loggerMock;
    private readonly InvoiceService _sut;

    public GetInvoicesTests()
    {
        _httpFactoryMock = new Mock<IHttpClientFactory>();
        _authMock = new Mock<IDataverseAuthService>();
        _options = Options.Create(new DataverseOptions
        {
            DataverseUrl = "https://test.crm.dynamics.com",
            TenantId = "test-tenant",
            ClientId = "test-client",
            ClientSecret = "test-secret"
        });
        _loggerMock = new Mock<ILogger<InvoiceService>>();

        _sut = new InvoiceService(
            _httpFactoryMock.Object,
            _authMock.Object,
            _options,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetInvoicesAsync_ReturnsInvoices_AndSetsAuthorizationHeader()
    {
        // Arrange
        var token = "test-token";

        _authMock.Setup(x => x.GetTokenAsync()).ReturnsAsync(token);

        var responseJson = @"{
            ""value"": [
                {
                    ""cr720_invoiceid"": ""11111111-1111-1111-1111-111111111111"",
                    ""cr720_invoicenumber"": ""INV-001"",
                    ""cr720_totalamount"": 1500.50,
                    ""cr720_status"": ""Paid"",
                    ""_cr720_customer_value"": ""aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"",
                    ""_cr720_customer_value@OData.Community.Display.V1.FormattedValue"": ""John Doe""
                },
                {
                    ""cr720_invoiceid"": ""22222222-2222-2222-2222-222222222222"",
                    ""cr720_invoicenumber"": ""INV-002"",
                    ""cr720_totalamount"": 2750.00,
                    ""cr720_status"": ""Pending"",
                    ""_cr720_customer_value"": ""bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"",
                    ""_cr720_customer_value@OData.Community.Display.V1.FormattedValue"": ""Jane Smith""
                }
            ]
        }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            })
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                req.Headers.Authorization.Should().NotBeNull();
                req.Headers.Authorization!.Scheme.Should().Be("Bearer");
                req.Headers.Authorization.Parameter.Should().Be(token);
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _sut.GetInvoicesAsync();

        // Assert
        result.Should().HaveCount(2);
        
        result[0].Id.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result[0].InvoiceNumber.Should().Be("INV-001");
        result[0].TotalAmount.Should().Be(1500.50m);
        result[0].Status.Should().Be("Paid");
        result[0].CustomerId.Should().Be(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        result[0].CustomerName.Should().Be("John Doe");

        result[1].Id.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        result[1].InvoiceNumber.Should().Be("INV-002");
        result[1].TotalAmount.Should().Be(2750.00m);
        result[1].Status.Should().Be("Pending");
        result[1].CustomerId.Should().Be(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        result[1].CustomerName.Should().Be("Jane Smith");

        _authMock.Verify(x => x.GetTokenAsync(), Times.Once);
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenApiReturnsError_ThrowsDataverseApiException()
    {
        // Arrange
        _authMock.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ReasonPhrase = "Unauthorized"
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var act = async () => await _sut.GetInvoicesAsync();

        // Assert
        await act.Should().ThrowAsync<DataverseApiException>()
            .WithMessage("*Dataverse API request failed: Unauthorized*");
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenInvalidJson_ThrowsDataverseApiException()
    {
        // Arrange
        _authMock.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ invalid json", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var act = async () => await _sut.GetInvoicesAsync();

        // Assert
        await act.Should().ThrowAsync<DataverseApiException>()
            .WithMessage("*Invalid JSON response from Dataverse*");
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenNetworkError_ThrowsDataverseApiException()
    {
        // Arrange
        _authMock.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handlerMock.Object);
        _httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var act = async () => await _sut.GetInvoicesAsync();

        // Assert
        await act.Should().ThrowAsync<DataverseApiException>()
            .WithMessage("*Network error occurred*");
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        _authMock.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");

        var responseJson = JsonSerializer.Serialize(new { value = Array.Empty<object>() });

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var result = await _sut.GetInvoicesAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
