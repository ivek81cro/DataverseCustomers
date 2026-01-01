# 🚗 VehicleDemo - Dataverse Customer & Invoice API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Azure Functions](https://img.shields.io/badge/Azure%20Functions-v4-0062AD?logo=azure-functions)](https://azure.microsoft.com/en-us/services/functions/)
[![Build](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-17%2F17%20passed-success)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

Azure Functions application for retrieving customers and invoices from Microsoft Dataverse with relational data support. Implements modern .NET 10 best practices with comprehensive error handling, structured logging, and extensive test coverage.

## 📋 Table of Contents

- [Features](#-features)
- [Technologies](#-technologies)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Running](#-running)
- [API Documentation](#-api-documentation)
- [Architecture](#-architecture)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Documentation](#-additional-documentation)
- [Contributing](#-contributing)

## ✨ Features

- 🔐 **OAuth 2.0 Authentication** - Service Principal authentication against Microsoft Entra ID
- 📊 **Dataverse Integration** - Retrieve data from Dataverse Web API
- 🔗 **Relational Data** - Support for OData $expand and lookup fields
- 📑 **Invoice Management** - Retrieve invoices with customer relationships
- 👥 **Customer Management** - Full CRUD-ready customer operations
- 🛡️ **Comprehensive Error Handling** - Custom exceptions and proper HTTP status codes
- 📝 **Structured Logging** - Application Insights integration with context
- 🧪 **Unit Tests** - 17 tests with FluentAssertions and Moq
- 🏗️ **Clean Architecture** - Repository pattern ready, SOLID principles
- ⚡ **Async/Await** - Fully asynchronous code
- 🔧 **Maintainable** - Constants, URL builders, dependency injection

## 🛠️ Technologies

| Component | Technology | Version |
|------------|-------------|---------|
| Runtime | .NET | 10.0 |
| Hosting | Azure Functions | v4 |
| Worker Model | Isolated Worker Process | Latest |
| Testing | xUnit + FluentAssertions | Latest |
| Mocking | Moq | Latest |
| Logging | Application Insights | 2.23.0 |
| HTTP | IHttpClientFactory | Built-in |

## 📦 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- Microsoft Dataverse environment
- Azure App Registration (Service Principal)

## 🚀 Installation

### 1. Clone Repository

```bash
git clone https://github.com/your-username/VehicleDemo.git
cd VehicleDemo
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build Solution

```bash
dotnet build
```

## ⚙️ Configuration

### Local Development

Create `local.settings.json` in the root directory:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    
    "TENANT_ID": "your-tenant-id",
    "CLIENT_ID": "your-client-id",
    "CLIENT_SECRET": "your-client-secret",
    "DATAVERSE_URL": "https://your-org.crm.dynamics.com"
  }
}
```

### Azure App Settings

For production deployment, set the same environment variables in Azure Portal:

```bash
az functionapp config appsettings set \
  --name your-function-app \
  --resource-group your-rg \
  --settings \
    TENANT_ID="your-tenant-id" \
    CLIENT_ID="your-client-id" \
    CLIENT_SECRET="@Microsoft.KeyVault(SecretUri=...)" \
    DATAVERSE_URL="https://your-org.crm.dynamics.com"
```

> ⚠️ **Security**: Use Azure Key Vault for sensitive data in production!

### Dataverse Setup

1. **Create App Registration** in Azure Portal
2. **Add API permissions**: Dynamics CRM → `user_impersonation`
3. **Create Application User** in Dataverse
4. **Assign Security Role** to application user

## 🏃 Running

### Local

```bash
# In root directory
func start
```

or in Visual Studio: **F5** for debug mode

The function will be available at:
```
http://localhost:7071/api/customers
http://localhost:7071/api/invoices
http://localhost:7071/api/customers/{customerId}/invoices
```

### Docker (Optional)

```bash
docker build -t vehicledemo .
docker run -p 8080:80 vehicledemo
```

## 📡 API Documentation

### Overview

The API provides three main endpoints for managing customers and their invoices:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/customers` | GET | Retrieve all active customers |
| `/api/invoices` | GET | Retrieve all invoices with customer info |
| `/api/customers/{customerId}/invoices` | GET | Retrieve invoices for specific customer |

---

### GET /api/customers

Retrieves all active customers from Dataverse.

#### Request

```http
GET /api/customers HTTP/1.1
Host: localhost:7071
```

#### Response Success (200 OK)

```json
[
  {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "name": "John Doe",
    "address": "123 Main Street",
    "email": "john.doe@example.com"
  },
  {
    "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "name": "Jane Smith",
    "address": "456 Oak Avenue",
    "email": "jane.smith@example.com"
  }
]
```

#### Response Errors

| Status Code | Description | Reason |
|-------------|------|--------|
| **502 Bad Gateway** | Dataverse API error | Network error, Invalid response, Auth failure |
| **500 Internal Server Error** | Unexpected error | Uncaught exceptions |

**Error Response Format:**

```json
{
  "error": "Failed to retrieve customers from Dataverse"
}
```

---

### GET /api/invoices

Retrieves all active invoices with related customer information using OData $expand.

#### Request

```http
GET /api/invoices HTTP/1.1
Host: localhost:7071
```

#### Response Success (200 OK)

```json
[
  {
    "id": "11111111-1111-1111-1111-111111111111",
    "invoiceNumber": "INV-001",
    "totalAmount": 1500.50,
    "status": "Paid",
    "customerId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "customerName": "John Doe"
  },
  {
    "id": "22222222-2222-2222-2222-222222222222",
    "invoiceNumber": "INV-002",
    "totalAmount": 2750.00,
    "status": "Pending",
    "customerId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "customerName": "Jane Smith"
  }
]
```

#### OData Query Details

The endpoint uses the following OData parameters:
- `$select` - Selects invoice fields and customer lookup
- `$filter` - Filters active records (statecode eq 0)
- `$expand` - Expands customer relationship to include customer name

#### Response Errors

| Status Code | Description | Reason |
|-------------|------|--------|
| **502 Bad Gateway** | Dataverse API error | Network error, Invalid response, Auth failure |
| **500 Internal Server Error** | Unexpected error | Uncaught exceptions |

---

### GET /api/customers/{customerId}/invoices

Retrieves all invoices for a specific customer.

#### Request

```http
GET /api/customers/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/invoices HTTP/1.1
Host: localhost:7071
```

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `customerId` | GUID | Yes | The unique identifier of the customer |

#### Response Success (200 OK)

```json
[
  {
    "id": "11111111-1111-1111-1111-111111111111",
    "invoiceNumber": "INV-001",
    "totalAmount": 1500.50,
    "status": "Paid",
    "customerId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "customerName": ""
  },
  {
    "id": "33333333-3333-3333-3333-333333333333",
    "invoiceNumber": "INV-003",
    "totalAmount": 899.99,
    "status": "Draft",
    "customerId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "customerName": ""
  }
]
```

> **Note**: When filtering by customer, the `customerName` field is empty as it's redundant.

#### Response Errors

| Status Code | Description | Reason |
|-------------|------|--------|
| **400 Bad Request** | Invalid customer ID | Customer ID is not a valid GUID |
| **502 Bad Gateway** | Dataverse API error | Network error, Invalid response, Auth failure |
| **500 Internal Server Error** | Unexpected error | Uncaught exceptions |

**Error Response Examples:**

```json
{
  "error": "Invalid customer ID format"
}
```

```json
{
  "error": "Failed to retrieve invoices from Dataverse"
}
```

## 🏛️ Architecture

### Project Structure

```
VehicleDemo/
├── Constants/
│   └── DataverseConstants.cs      # Dataverse field names, entities, filters
├── Exceptions/
│   └── DataverseApiException.cs   # Custom exception for Dataverse errors
├── Models/
│   ├── CustomerDto.cs             # Customer data transfer object
│   └── InvoiceDto.cs              # Invoice data transfer object
├── Services/
│   ├── DataverseAuthService.cs    # OAuth token management
│   └── DataverseUrlBuilder.cs     # OData URL construction logic
├── GetCustomers.cs                # Azure Function: GET /api/customers
├── GetInvoices.cs                 # Azure Function: GET /api/invoices
├── GetInvoicesByCustomer.cs       # Azure Function: GET /api/customers/{id}/invoices
├── Program.cs                     # DI configuration
└── host.json                      # Functions host configuration

VehicleDemo.Tests/
├── GetCustomersTests.cs           # Unit tests for customers (4 tests)
├── GetInvoicesTests.cs            # Unit tests for invoices (5 tests)
├── GetInvoicesByCustomerTests.cs  # Unit tests for customer invoices (5 tests)
└── DataverseUrlBuilderTests.cs    # Unit tests for URL builder (3 tests)
```

### Flow Diagram

#### Customer Retrieval Flow
```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │ GET /api/customers
       ▼
┌──────────────────────────┐
│  GetCustomers Function   │
└──────┬───────────────────┘
       │
       ├─► DataverseAuthService ──► Entra ID (OAuth Token)
       │
       ├─► DataverseUrlBuilder   ──► Build OData Query
       │
       ▼
┌──────────────────────────┐
│  Dataverse Web API       │
│  /api/data/v9.2/         │
│  cr720_customers         │
└──────┬───────────────────┘
       │ JSON Response
       ▼
┌──────────────────────────┐
│  CustomerDto Mapping     │
└──────┬───────────────────┘
       │
       ▼ Return List<CustomerDto>
```

#### Invoice with Customer Relationship Flow
```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │ GET /api/invoices
       ▼
┌──────────────────────────┐
│  GetInvoices Function    │
└──────┬───────────────────┘
       │
       ├─► DataverseAuthService ──► Entra ID (OAuth Token)
       │
       ├─► DataverseUrlBuilder   ──► Build OData Query with $expand
       │
       ▼
┌──────────────────────────┐
│  Dataverse Web API       │
│  /api/data/v9.2/         │
│  cr720_invoices?         │
│  $expand=cr720_customer  │
└──────┬───────────────────┘
       │ JSON with Customer Info
       ▼
┌──────────────────────────┐
│  InvoiceDto Mapping      │
│  (includes CustomerName) │
└──────┬───────────────────┘
       │
       ▼ Return List<InvoiceDto>
```

#### Customer-Filtered Invoices Flow
```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │ GET /api/customers/{id}/invoices
       ▼
┌─────────────────────────────┐
│ GetInvoicesByCustomer Func  │
│ - Validate GUID              │
└──────┬──────────────────────┘
       │
       ├─► DataverseAuthService ──► Entra ID (OAuth Token)
       │
       ├─► DataverseUrlBuilder   ──► Build OData Query with $filter
       │
       ▼
┌──────────────────────────┐
│  Dataverse Web API       │
│  /api/data/v9.2/         │
│  cr720_invoices?         │
│  $filter=_cr720_         │
│  customer_value eq {id}  │
└──────┬───────────────────┘
       │ Filtered JSON
       ▼
┌──────────────────────────┐
│  InvoiceDto Mapping      │
└──────┬───────────────────┘
       │
       ▼ Return List<InvoiceDto>
```

### Design Patterns

- ✅ **Dependency Injection** - Constructor injection for testability
- ✅ **Factory Pattern** - IHttpClientFactory for HTTP client management
- ✅ **Builder Pattern** - DataverseUrlBuilder for OData query construction
- ✅ **Constants Pattern** - DataverseConstants for field names and entities
- ✅ **DTO Pattern** - Separate data transfer objects (CustomerDto, InvoiceDto)
- ✅ **Error Handling Pattern** - Custom exceptions with proper HTTP status codes
- 🔄 **Repository Pattern** - Ready for implementation

### Key Technical Decisions

#### OData $expand vs Multiple Requests
- **Decision**: Use `$expand` for invoice-customer relationships
- **Rationale**: Reduces network calls, improves performance, atomic data consistency
- **Implementation**: `BuildInvoicesUrl()` includes `$expand=cr720_customer($select=cr720_customername)`

#### Lookup Field Naming Convention
- **Dataverse Pattern**: Lookup fields use `_fieldname_value` format
- **Example**: `_cr720_customer_value` for Customer lookup in Invoice
- **OData Annotation**: `@OData.Community.Display.V1.FormattedValue` for display names

#### GUID Validation in Route Parameters
- **Decision**: Validate GUID format before API call
- **Rationale**: Early validation, better error messages, prevent invalid Dataverse queries
- **Implementation**: `Guid.TryParse()` with 400 Bad Request response

## 🧪 Testing

### Run All Tests

```bash
dotnet test VehicleDemo.Tests/VehicleDemo.Tests.csproj
```

### Run with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Summary

**Total: 17 tests - All passing ✅**

| Test Suite | Tests | Coverage |
|------------|-------|----------|
| GetCustomersTests | 4 | Customer retrieval scenarios |
| GetInvoicesTests | 5 | Invoice retrieval with relationships |
| GetInvoicesByCustomerTests | 5 | Customer-filtered invoice scenarios |
| DataverseUrlBuilderTests | 3 | OData URL construction |

---

### GetCustomersTests (4 tests)

| Test | Scenario | Status |
|------|----------|--------|
| `GetCustomersAsync_ReturnsCustomers_AndSetsAuthorizationHeader` | Happy path with auth validation | ✅ |
| `GetCustomersAsync_WhenApiReturnsError_ThrowsDataverseApiException` | HTTP error handling | ✅ |
| `GetCustomersAsync_WhenInvalidJson_ThrowsDataverseApiException` | Invalid JSON response | ✅ |
| `GetCustomersAsync_WhenNetworkError_ThrowsDataverseApiException` | Network failure | ✅ |

---

### GetInvoicesTests (5 tests)

| Test | Scenario | Status |
|------|----------|--------|
| `GetInvoicesAsync_ReturnsInvoices_AndSetsAuthorizationHeader` | Happy path with OData expansion | ✅ |
| `GetInvoicesAsync_WhenApiReturnsError_ThrowsDataverseApiException` | HTTP 401 Unauthorized | ✅ |
| `GetInvoicesAsync_WhenInvalidJson_ThrowsDataverseApiException` | Malformed JSON | ✅ |
| `GetInvoicesAsync_WhenNetworkError_ThrowsDataverseApiException` | Network exception | ✅ |
| `GetInvoicesAsync_WhenEmptyResponse_ReturnsEmptyList` | No invoices found | ✅ |

**Key Test Features:**
- ✅ Validates OData `@OData.Community.Display.V1.FormattedValue` parsing
- ✅ Tests customer relationship data in invoice response
- ✅ Verifies decimal amount handling (1500.50, 2750.00)

---

### GetInvoicesByCustomerTests (5 tests)

| Test | Scenario | Status |
|------|----------|--------|
| `GetInvoicesByCustomerIdAsync_ReturnsInvoicesForSpecificCustomer` | Customer-filtered results | ✅ |
| `GetInvoicesByCustomerIdAsync_WhenNoInvoicesFound_ReturnsEmptyList` | Customer with no invoices | ✅ |
| `GetInvoicesByCustomerIdAsync_WhenApiReturnsError_ThrowsDataverseApiException` | HTTP 404 Not Found | ✅ |
| `GetInvoicesByCustomerIdAsync_WhenInvalidJson_ThrowsDataverseApiException` | Invalid response format | ✅ |
| `GetInvoicesByCustomerIdAsync_WhenNetworkError_ThrowsDataverseApiException` | Connection refused | ✅ |

**Key Test Features:**
- ✅ Validates GUID in URL filter (`$filter=_cr720_customer_value eq {guid}`)
- ✅ Verifies Bearer token authorization
- ✅ Tests empty CustomerName when filtering by customer

---

### DataverseUrlBuilderTests (3 tests)

| Test | Scenario | Status |
|------|----------|--------|
| `BuildInvoicesUrl_ReturnsCorrectUrlWithExpandAndFilter` | OData $expand validation | ✅ |
| `BuildInvoicesByCustomerUrl_ReturnsCorrectUrlWithCustomerFilter` | OData $filter with GUID | ✅ |
| `BuildInvoicesByCustomerUrl_WithDifferentGuids_GeneratesDifferentUrls` | GUID uniqueness | ✅ |

**Key Test Features:**
- ✅ Validates `$expand=cr720_customer($select=cr720_customername)`
- ✅ Validates `$filter=statecode eq 0 and _cr720_customer_value eq {guid}`
- ✅ Ensures no $expand in customer-filtered queries

---

### Test Implementation Patterns

#### Mocking HttpClient with HttpMessageHandler
```csharp
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
```

#### Testing OData Formatted Values
```csharp
// Raw JSON string to support @ symbol in property names
var responseJson = @"{
    ""_cr720_customer_value"": ""guid"",
    ""_cr720_customer_value@OData.Community.Display.V1.FormattedValue"": ""John Doe""
}";
```

#### Authorization Header Validation
```csharp
.Callback<HttpRequestMessage, CancellationToken>((req, _) =>
{
    req.Headers.Authorization.Should().NotBeNull();
    req.Headers.Authorization!.Scheme.Should().Be("Bearer");
    req.Headers.Authorization.Parameter.Should().Be(token);
});
```

---

### Coverage Report

Run with detailed coverage:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Current Coverage:**
- Services: 100%
- Functions: 95%
- Constants: 100%
- Models: 100%

## 📦 Deployment

### Azure Portal

1. Create Function App (Linux, .NET 10 Isolated)
2. Configure App Settings
3. Deploy from Visual Studio:
   - Right-click on project → Publish → Azure

### Azure CLI

```bash
# Login
az login

# Create Resource Group
az group create --name rg-vehicledemo --location westeurope

# Create Storage Account
az storage account create \
  --name stvehicledemo \
  --resource-group rg-vehicledemo \
  --location westeurope \
  --sku Standard_LRS

# Create Function App
az functionapp create \
  --name func-vehicledemo \
  --resource-group rg-vehicledemo \
  --storage-account stvehicledemo \
  --consumption-plan-location westeurope \
  --runtime dotnet-isolated \
  --runtime-version 10 \
  --functions-version 4 \
  --os-type Linux

# Deploy
func azure functionapp publish func-vehicledemo
```

### GitHub Actions (CI/CD)

```yaml
name: Deploy to Azure Functions

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Build
        run: dotnet build --configuration Release
      
      - name: Test
        run: dotnet test --no-build --configuration Release
      
      - name: Publish
        run: dotnet publish --configuration Release --output ./output
      
      - name: Deploy to Azure
        uses: Azure/functions-action@v1
        with:
          app-name: func-vehicledemo
          package: ./output
          publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
```

## 📊 Monitoring

### Application Insights

The application automatically sends telemetry to Application Insights:

- **Requests** - HTTP request duration and status codes
- **Dependencies** - Dataverse API calls, Auth calls
- **Exceptions** - Caught and uncaught exceptions
- **Traces** - Structured logs with context

### Query Examples

```kusto
// Failed requests in last 24h
requests
| where timestamp > ago(24h)
| where success == false
| summarize count() by resultCode, name

// Dataverse API performance
dependencies
| where type == "Http"
| where target contains "dynamics.com"
| summarize avg(duration), percentile(duration, 95) by bin(timestamp, 5m)
```

## 🔐 Security Best Practices

- ✅ Client secrets in Azure Key Vault
- ✅ Managed Identity for production
- ✅ Minimal Dataverse permissions
- ✅ No secrets in source control
- ✅ HTTPS only
- ❌ Authorization level: Anonymous (add auth for production!)

> ⚠️ **Important**: `AuthorizationLevel.Anonymous` is for development only! For production use `Function` or `Admin` level + Azure AD authentication.

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. **Fork** the repository
2. **Create branch** (`git checkout -b feature/amazing-feature`)
3. **Commit** changes (`git commit -m 'Add amazing feature'`)
4. **Push** to branch (`git push origin feature/amazing-feature`)
5. **Open Pull Request**

### Code Style

- Use C# naming conventions
- Add XML documentation for public API
- Tests for all new features
- Run `dotnet format` before committing

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📚 Additional Documentation

> **Note:** Comprehensive documentation is available in the `Documentation/` folder (not tracked in Git).

For local development, refer to:
- 📘 Implementation Guide - Detailed technical guide
- 📋 Changelog - Version history and changes
- 🔧 Troubleshooting - Common issues and solutions
- 📖 Master Index - Complete documentation navigation
- 🎯 Project Status - Current project state

---

## 🙏 Acknowledgments

- Microsoft Dataverse team for excellent documentation
- Azure Functions team for isolated worker model
- .NET community for best practices
- OData protocol contributors
