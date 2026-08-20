# Шаг 8. Тестирование

---

## 8.1. Создание тестовых проектов

```powershell
# Из корня решения
mkdir tests

dotnet new xunit -n TechnicalSupportService.UnitTests -o tests/TechnicalSupportService.UnitTests
dotnet new xunit -n TechnicalSupportService.IntegrationTests -o tests/TechnicalSupportService.IntegrationTests

dotnet sln add tests/TechnicalSupportService.UnitTests/TechnicalSupportService.UnitTests.csproj
dotnet sln add tests/TechnicalSupportService.IntegrationTests/TechnicalSupportService.IntegrationTests.csproj

# Ссылки для UnitTests
dotnet add tests/TechnicalSupportService.UnitTests/TechnicalSupportService.UnitTests.csproj reference TechnicalSupportService.Core/TechnicalSupportService.Core.csproj
dotnet add tests/TechnicalSupportService.UnitTests/TechnicalSupportService.UnitTests.csproj reference TechnicalSupportService.Data/TechnicalSupportService.Data.csproj
dotnet add tests/TechnicalSupportService.UnitTests/TechnicalSupportService.UnitTests.csproj reference TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj

# Ссылки для IntegrationTests
dotnet add tests/TechnicalSupportService.IntegrationTests/TechnicalSupportService.IntegrationTests.csproj reference TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj

# NuGet для IntegrationTests
dotnet add tests/TechnicalSupportService.IntegrationTests/TechnicalSupportService.IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/TechnicalSupportService.IntegrationTests/TechnicalSupportService.IntegrationTests.csproj package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/TechnicalSupportService.IntegrationTests/TechnicalSupportService.IntegrationTests.csproj package Moq
```

---

## 8.2. Unit-тест нумерации

### tests/TechnicalSupportService.UnitTests/Services/NumberGeneratorServiceTests.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.SUTP.Services;

namespace TechnicalSupportService.UnitTests.Services;

public class NumberGeneratorServiceTests : IDisposable
{
    private readonly ApplicationDbContext _db;

    public NumberGeneratorServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GenerateNextNumber_First_Returns001()
    {
        var service = new NumberGeneratorService(_db);
        var number = await service.GenerateNextNumberAsync();

        var prefix = DateTime.UtcNow.ToString("yyyy_MM");
        Assert.StartsWith(prefix, number);
        Assert.EndsWith("_001", number);
    }

    [Fact]
    public async Task GenerateNextNumber_Sequential_Returns002()
    {
        var service = new NumberGeneratorService(_db);

        await service.GenerateNextNumberAsync();
        var number = await service.GenerateNextNumberAsync();

        Assert.EndsWith("_002", number);
    }

    [Fact]
    public async Task GenerateNextNumber_Multiple_ReturnsCorrectSequence()
    {
        var service = new NumberGeneratorService(_db);

        for (int i = 0; i < 5; i++)
            await service.GenerateNextNumberAsync();

        var number = await service.GenerateNextNumberAsync();
        Assert.EndsWith("_006", number);
    }
}
```

---

## 8.3. Unit-тест бизнес-логики

### tests/TechnicalSupportService.UnitTests/Services/TicketServiceTests.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;
using TechnicalSupportService.SUTP.Services;

namespace TechnicalSupportService.UnitTests.Services;

public class TicketServiceTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _engineerId = Guid.NewGuid();
    private readonly Guid _applicantId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public TicketServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        SeedTestData();
    }

    private void SeedTestData()
    {
        _db.Products.Add(new Product { Id = _productId, Name = "Test Product", ProductType = ProductType.Software });
        _db.TicketNumberCounters.Add(new TicketNumberCounter { YearMonth = DateTime.UtcNow.ToString("yyyy-MM"), LastNumber = 0 });
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private TicketService CreateService()
    {
        var numberGen = new NumberGeneratorService(_db);
        var audit = new AuditService(_db);
        return new TicketService(_db, numberGen, audit);
    }

    [Fact]
    public async Task CreateAsync_Valid_ReturnsTicketWithNumber()
    {
        var service = CreateService();
        var dto = new TicketCreateDto
        {
            Title = "Test Ticket",
            Description = "Description",
            ProductId = _productId,
            Priority = Priority.Medium,
            Category = Category.Bug,
            Source = Source.Portal
        };

        var result = await service.CreateAsync(dto, _applicantId);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Number);
        Assert.Equal("Test Ticket", result.Title);
        Assert.Equal(TicketStatus.New, result.Status);
    }

    [Fact]
    public async Task ChangeStatusAsync_InvalidTransition_Throws()
    {
        var service = CreateService();
        var dto = new TicketCreateDto
        {
            Title = "Test", Description = "Desc", ProductId = _productId,
            Priority = Priority.Low, Category = Category.Bug, Source = Source.Portal
        };
        var ticket = await service.CreateAsync(dto, _applicantId);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.ChangeStatusAsync(ticket.Id, TicketStatus.Resolved, null, _engineerId));
    }

    [Fact]
    public async Task DeleteAsync_NonAdmin_ThrowsForbidden()
    {
        var service = CreateService();
        var dto = new TicketCreateDto
        {
            Title = "Test", Description = "Desc", ProductId = _productId,
            Priority = Priority.Low, Category = Category.Bug, Source = Source.Portal
        };
        var ticket = await service.CreateAsync(dto, _applicantId);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.DeleteAsync(ticket.Id, _applicantId));
    }
}
```

---

## 8.4. Integration-тест

### tests/TechnicalSupportService.IntegrationTests/CustomWebApplicationFactory.cs

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechnicalSupportService.Data.Context;

namespace TechnicalSupportService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }
}
```

### tests/TechnicalSupportService.IntegrationTests/Controllers/AccountControllerTests.cs

```csharp
namespace TechnicalSupportService.IntegrationTests.Controllers;

public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Get_ReturnsOk()
    {
        var response = await _client.GetAsync("/Account/Login");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Dashboard_WithoutAuth_RedirectsToLogin()
    {
        var client = new CustomWebApplicationFactory().CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Dashboard");
        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
    }
}
```

---

## 8.5. Запуск тестов

```powershell
dotnet test
```
