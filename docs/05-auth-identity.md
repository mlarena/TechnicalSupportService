# Шаг 5. Identity, DI, Program.cs и Seed-данные

> Все файлы — в проекте TechnicalSupportService.SUTP.

---

## 5.1. appsettings.json

### SUTP/appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=support_db;Username=postgres;Password=postgres"
  },
  "FileStorage": {
    "LocalPath": "C:\\dev\\TechnicalSupportService\\files",
    "MaxFileSizeBytes": 52428800,
    "MaxTotalSizePerTicketBytes": 524288000,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".png", ".zip"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### SUTP/appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=support_db;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

---

## 5.2. Constants (Core/Constants/)

### Core/Constants/Roles.cs

```csharp
namespace TechnicalSupportService.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Engineer = "Engineer";
    public const string Manager = "Manager";
    public const string Applicant = "Applicant";

    public static readonly string[] All = { Admin, Engineer, Manager, Applicant };
}
```

---

## 5.3. Infrastructure/ServiceCollectionExtensions.cs

```csharp
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.SUTP.Services;

namespace TechnicalSupportService.SUTP.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<INumberGeneratorService, NumberGeneratorService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
```

---

## 5.4. Program.cs (полный файл)

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;
using TechnicalSupportService.SUTP.Infrastructure;
using TechnicalSupportService.SUTP.Middleware;

var builder = WebApplication.CreateBuilder(args);

// === БД ===
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// === Identity ===
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 4;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// === Cookie ===
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// === MVC ===
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// === Бизнес-сервисы ===
builder.Services.AddApplicationServices();

// === Antiforgery ===
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

var app = builder.Build();

// === Seed данных ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}

// === Pipeline ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

// Для интеграционных тестов
public partial class Program { }
```

---

## 5.5. Infrastructure/SeedData.cs (с 4 тестовыми пользователями)

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Infrastructure;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await SeedRolesAsync(serviceProvider);
        await SeedTestUsersAsync(serviceProvider);
        await SeedDepartmentsAsync(serviceProvider);
        await SeedProductsAsync(serviceProvider);
    }

    private static async Task SeedRolesAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }
    }

    private static async Task SeedTestUsersAsync(IServiceProvider sp)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        var users = new[]
        {
            new { Email = "admin@company.com", Password = "Admin@123", FullName = "Администратор Системы", Role = Roles.Admin, Position = "Системный администратор" },
            new { Email = "engineer@company.com", Password = "Engineer@123", FullName = "Инженер Техподдержки", Role = Roles.Engineer, Position = "Инженер" },
            new { Email = "manager@company.com", Password = "Manager@123", FullName = "Менеджер Проектов", Role = Roles.Manager, Position = "Менеджер" },
            new { Email = "applicant@company.com", Password = "Applicant@123", FullName = "Иван Заявитель", Role = Roles.Applicant, Position = "Сотрудник" }
        };

        foreach (var u in users)
        {
            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing != null) continue;

            var user = new ApplicationUser
            {
                UserName = u.Email,
                Email = u.Email,
                FullName = u.FullName,
                Position = u.Position,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, u.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, u.Role);
            }
        }
    }

    private static async Task SeedDepartmentsAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.Departments.AnyAsync()) return;

        db.Departments.AddRange(
            new Department { Name = "IT-отдел", Description = "Информационные технологии" },
            new Department { Name = "Отдел разработки", Description = "Разработка ПО" },
            new Department { Name = "Отдел продаж", Description = "Продажи и маркетинг" }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.Products.AnyAsync()) return;

        db.Products.AddRange(
            new Product { Name = "CRM v3.2", ProductType = ProductType.Software, CurrentVersion = "3.2.1", Description = "Система управления клиентами" },
            new Product { Name = "Контроллер Т-100", ProductType = ProductType.Hardware, CurrentVersion = "2.0", Description = "Промышленный контроллер" },
            new Product { Name = "Встраиваемый модуль M1", ProductType = ProductType.Embedded, CurrentVersion = "1.5.3", Description = "Встраиваемый вычислительный модуль" }
        );

        await db.SaveChangesAsync();
    }
}
```

---

## 5.6. Middleware/ExceptionHandlingMiddleware.cs

```csharp
using System.Net;
using System.Text.Json;
using TechnicalSupportService.Core.Exceptions;

namespace TechnicalSupportService.SUTP.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteErrorResponse(context, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Forbidden");
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await WriteErrorResponse(context, ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteErrorResponse(context, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteErrorResponse(context, "Внутренняя ошибка сервера");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        var response = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(response);
    }
}
```

---

## 5.7. Filters/AuditActionFilter.cs

```csharp
using Microsoft.AspNetCore.Mvc.Filters;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Filters;

public class AuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditService _auditService;

    public AuditActionFilter(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            var controllerName = context.Controller.GetType().Name;
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

            await _auditService.LogAsync(
                $"{controllerName}.{actionName}",
                Guid.Parse(userId),
                ipAddress: ipAddress,
                userAgent: userAgent);
        }

        await next();
    }
}
```
