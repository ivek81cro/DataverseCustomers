# 🚗 VehicleDemo - Dataverse Customer API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Azure Functions](https://img.shields.io/badge/Azure%20Functions-v4-0062AD?logo=azure-functions)](https://azure.microsoft.com/en-us/services/functions/)
[![Build](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-4%2F4%20passed-success)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

Azure Functions application for retrieving customers from Microsoft Dataverse. Implements modern .NET 10 best practices with comprehensive error handling, structured logging, and tests.

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
- [Contributing](#-contributing)

## ✨ Features

- 🔐 **OAuth 2.0 Authentication** - Service Principal authentication against Microsoft Entra ID
- 📊 **Dataverse Integration** - Retrieve data from Dataverse Web API
- 🛡️ **Comprehensive Error Handling** - Custom exceptions and proper HTTP status codes
- 📝 **Structured Logging** - Application Insights integration
- 🧪 **Unit Tests** - 4 tests with FluentAssertions
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
```

### Docker (Optional)

```bash
docker build -t vehicledemo .
docker run -p 8080:80 vehicledemo
```

## 📡 API Documentation

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

## 🏛️ Architecture

### Project Structure

```
VehicleDemo/
├── Constants/
│   └── DataverseConstants.cs      # Dataverse field names, entities
├── Exceptions/
│   └── DataverseApiException.cs   # Custom exception
├── Models/
│   └── CustomerDto.cs             # Customer data model
├── Services/
│   ├── DataverseAuthService.cs    # OAuth token management
│   └── DataverseUrlBuilder.cs     # URL construction logic
├── GetCustomers.cs                # Azure Function endpoint
├── Program.cs                     # DI configuration
└── host.json                      # Functions host config

VehicleDemo.Tests/
└── GetCustomersTests.cs           # Unit tests (4 scenarios)
```

### Flow Diagram

```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │ GET /api/customers
       ▼
┌──────────────────────────┐
│  GetCustomers Function   │
│  (Azure Function)        │
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
└──────┬───────────────────┘
       │ JSON Response
       ▼
┌──────────────────────────┐
│  CustomerDto Mapping     │
└──────┬───────────────────┘
       │
       ▼ Return List<CustomerDto>
```

### Design Patterns

- ✅ **Dependency Injection** - Constructor injection
- ✅ **Factory Pattern** - IHttpClientFactory
- ✅ **Builder Pattern** - DataverseUrlBuilder
- ✅ **Constants Pattern** - DataverseConstants
- 🔄 **Repository Pattern** - Ready for implementation

## 🧪 Testing

### Run All Tests

```bash
dotnet test
```

### Run with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Scenarios

| Test | Scenario | Status |
|------|----------|--------|
| `GetCustomersAsync_ReturnsCustomers_AndSetsAuthorizationHeader` | Happy path | ✅ |
| `GetCustomersAsync_WhenApiReturnsError_ThrowsDataverseApiException` | HTTP error | ✅ |
| `GetCustomersAsync_WhenInvalidJson_ThrowsDataverseApiException` | Invalid JSON | ✅ |
| `GetCustomersAsync_WhenNetworkError_ThrowsDataverseApiException` | Network failure | ✅ |

**Coverage**: 4/4 tests passing

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

## 🙏 Acknowledgments

- Microsoft Dataverse team for excellent documentation
- Azure Functions team for isolated worker model
- .NET community for best practices
