# Шаг 5. Настройка Identity, DI и конфигурация приложения

## 5.1. ASP.NET Core Identity — настройка

### Кастомная сущность пользователя

```csharp
// TechnicalSupportService.Data/Entities/ApplicationUser.cs
public class ApplicationUser : IdentityUser<Guid>
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public Guid? DepartmentId { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }
}
```

### Кастомный DbContext

```csharp
// TechnicalSupportService.Data/Context/ApplicationDbContext.cs
public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<TicketNumberCounter> TicketNumberCounters => Set<TicketNumberCounter>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

### Регистрация Identity в Program.cs

```csharp
// TechnicalSupportService.SUTP/Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Настройки паролей
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 4;

    // Настройки блокировки
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Настройки пользователя
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Подтверждение email (опционально)
    options.SignIn.RequireConfirmedEmail = false; // true в продакшене
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

---

## 5.2. Регистрация ролей

### Seed ролей

```csharp
// TechnicalSupportService.SUTP/Infrastructure/SeedData.cs
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider
            .GetRequiredService<RoleManager<ApplicationRole>>();

        string[] roles = { "Admin", "Engineer", "Manager", "Applicant" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    NormalizedName = role.ToUpper()
                });
            }
        }
    }
}
```

### Seed администратора

```csharp
public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    const string adminEmail = "admin@company.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Администратор системы",
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
```

### Константы ролей

```csharp
// TechnicalSupportService.Core/Constants/Roles.cs
public static class Roles
{
    public const string Admin = "Admin";
    public const string Engineer = "Engineer";
    public const string Manager = "Manager";
    public const string Applicant = "Applicant";
}
```

---

## 5.3. Dependency Injection — полная регистрация

```csharp
// TechnicalSupportService.SUTP/Infrastructure/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Сервисы бизнес-логики
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

        // Фильтры
        services.AddScoped<AuditActionFilter>();

        return services;
    }
}
```

---

## 5.4. Конфигурация appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=support_db;Username=app_user;Password=***"
  },
  "FileStorage": {
    "Provider": "Local",
    "LocalPath": "/var/data/support-files",
    "MaxFileSizeBytes": 52428800,
    "MaxTotalSizePerTicketBytes": 524288000,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".png", ".zip"]
  },
  "TicketNumber": {
    "NumberFormat": "{0:0000}_{1:00}_{2:D3}"
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

---

## 5.5. Конфигурация Services (DI-контейнер) в Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Runtime Compilation
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => { ... })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Бизнес-сервисы
builder.Services.AddApplicationServices();

// Анти-CSRF
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

var app = builder.Build();

// Seed ролей и администратора
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
    await SeedData.SeedAdminUserAsync(scope.ServiceProvider);
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
```
