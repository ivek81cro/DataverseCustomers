using FluentAssertions;
using VehicleDemo.Services;
using Xunit;

namespace VehicleDemo.Tests;

public class DataverseUrlBuilderTests
{
    private const string BaseUrl = "https://test.crm.dynamics.com";
    private const string ApiVersion = "v9.2";

    [Fact]
    public void BuildCustomersUrl_ReturnsCorrectUrl()
    {
        // Act
        var result = DataverseUrlBuilder.BuildCustomersUrl(BaseUrl);

        // Assert
        result.Should().StartWith($"{BaseUrl}/api/data/{ApiVersion}/cr720_customers");
        result.Should().Contain("$select=cr720_customerid,cr720_customername,cr720_address,cr720_email");
        result.Should().Contain("$filter=statecode eq 0");
    }

    [Fact]
    public void BuildInvoicesUrl_ReturnsCorrectUrl()
    {
        // Act
        var result = DataverseUrlBuilder.BuildInvoicesUrl(BaseUrl);

        // Assert
        result.Should().StartWith($"{BaseUrl}/api/data/{ApiVersion}/cr720_invoices");
        result.Should().Contain("$select=");
        result.Should().Contain("cr720_invoiceid");
        result.Should().Contain("cr720_invoicenumber");
        result.Should().Contain("cr720_totalamount");
        result.Should().Contain("cr720_status");
        result.Should().Contain("_cr720_customer_value");
        result.Should().Contain("$filter=statecode eq 0");
    }

    [Fact]
    public void BuildInvoicesByCustomerUrl_ReturnsCorrectUrlWithCustomerFilter()
    {
        // Arrange
        var customerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Act
        var result = DataverseUrlBuilder.BuildInvoicesByCustomerUrl(BaseUrl, customerId);

        // Assert
        result.Should().StartWith($"{BaseUrl}/api/data/{ApiVersion}/cr720_invoices");
        result.Should().Contain("$select=");
        result.Should().Contain("cr720_invoiceid");
        result.Should().Contain("cr720_invoicenumber");
        result.Should().Contain("cr720_totalamount");
        result.Should().Contain("cr720_status");
        result.Should().Contain("_cr720_customer_value");
        result.Should().Contain($"$filter=statecode eq 0 and _cr720_customer_value eq {customerId}");
        result.Should().NotContain("$expand"); // Should not use expand in customer-filtered query
    }

    [Fact]
    public void BuildInvoicesByCustomerUrl_WithDifferentGuids_GeneratesDifferentUrls()
    {
        // Arrange
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        // Act
        var url1 = DataverseUrlBuilder.BuildInvoicesByCustomerUrl(BaseUrl, customerId1);
        var url2 = DataverseUrlBuilder.BuildInvoicesByCustomerUrl(BaseUrl, customerId2);

        // Assert
        url1.Should().NotBe(url2);
        url1.Should().Contain(customerId1.ToString());
        url2.Should().Contain(customerId2.ToString());
    }

    [Fact]
    public void BuildInvoicesByCustomerUrl_DoesNotIncludeFormattedValueAnnotationInSelect()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var result = DataverseUrlBuilder.BuildInvoicesByCustomerUrl(BaseUrl, customerId);

        // Assert
        result.Should().NotContain("@OData.Community.Display.V1.FormattedValue");
        result.Should().Contain("_cr720_customer_value");
    }

    [Theory]
    [InlineData("https://org1.crm.dynamics.com")]
    [InlineData("https://org2.crm4.dynamics.com")]
    [InlineData("https://custom-org.crm.dynamics.com")]
    public void BuildCustomersUrl_WorksWithDifferentBaseUrls(string baseUrl)
    {
        // Act
        var result = DataverseUrlBuilder.BuildCustomersUrl(baseUrl);

        // Assert
        result.Should().StartWith($"{baseUrl}/api/data/{ApiVersion}/cr720_customers");
        result.Should().Contain("$select=");
        result.Should().Contain("$filter=");
    }

    [Fact]
    public void BuildInvoicesUrl_ContainsAllRequiredFields()
    {
        // Act
        var result = DataverseUrlBuilder.BuildInvoicesUrl(BaseUrl);

        // Assert
        var requiredFields = new[]
        {
            "cr720_invoiceid",
            "cr720_invoicenumber",
            "cr720_totalamount",
            "cr720_status",
            "_cr720_customer_value"
        };

        foreach (var field in requiredFields)
        {
            result.Should().Contain(field, $"URL should contain field {field}");
        }
    }

    [Fact]
    public void BuildInvoicesByCustomerUrl_ContainsAllRequiredFields()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var result = DataverseUrlBuilder.BuildInvoicesByCustomerUrl(BaseUrl, customerId);

        // Assert
        var requiredFields = new[]
        {
            "cr720_invoiceid",
            "cr720_invoicenumber",
            "cr720_totalamount",
            "cr720_status",
            "_cr720_customer_value"
        };

        foreach (var field in requiredFields)
        {
            result.Should().Contain(field, $"URL should contain field {field}");
        }
    }

    [Fact]
    public void AllBuildMethods_IncludeActiveRecordsFilter()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var customersUrl = DataverseUrlBuilder.BuildCustomersUrl(BaseUrl);
        var invoicesUrl = DataverseUrlBuilder.BuildInvoicesUrl(BaseUrl);
        var invoicesByCustomerUrl = DataverseUrlBuilder.BuildInvoicesByCustomerUrl(BaseUrl, customerId);

        // Assert
        customersUrl.Should().Contain("statecode eq 0");
        invoicesUrl.Should().Contain("statecode eq 0");
        invoicesByCustomerUrl.Should().Contain("statecode eq 0");
    }
}