# Шаг 7. Конфигурация EF Core, миграции, seed-данные

## 7.1. Fluent API-конфигурации

Все конфигурации — в `TechnicalSupportService.Data/Configurations/`, по одному файлу на сущность.

### TicketConfiguration

```csharp
// TechnicalSupportService.Data/Configurations/TicketConfiguration.cs
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Number)
            .IsUnique();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired();

        builder.Property(t => t.Version)
            .HasMaxLength(20);

        builder.Property(t => t.Resolution);

        builder.Property(t => t.TimeSpentMinutes);

        // Enum → строка в БД
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Category)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Impact)
            .HasConversion<string?>()
            .HasMaxLength(20);

        builder.Property(t => t.Source)
            .HasConversion<string>()
            .HasMaxLength(20);

        // FK → Products
        builder.HasOne(t => t.Product)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK → Users (создатель)
        builder.HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK → Users (исполнитель)
        builder.HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK → Users (обновивший)
        builder.HasOne(t => t.UpdatedByUser)
            .WithMany()
            .HasForeignKey(t => t.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Самоссылка (ParentTicket)
        builder.HasOne(t => t.ParentTicket)
            .WithMany(t => t.ChildTickets)
            .HasForeignKey(t => t.ParentTicketId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => t.CreatedByUserId);
        builder.HasIndex(t => t.ProductId);
    }
}
```

### TicketHistoryConfiguration

```csharp
public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FieldName)
            .HasMaxLength(50);

        builder.Property(h => h.ChangeType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(h => h.Ticket)
            .WithMany(t => t.History)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedByUser)
            .WithMany()
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Comment)
            .WithMany()
            .HasForeignKey(h => h.CommentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.Attachment)
            .WithMany()
            .HasForeignKey(h => h.AttachmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(h => new { h.TicketId, h.ChangedAt });
    }
}
```

### CommentConfiguration

```csharp
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .IsRequired();

        builder.HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AuthorUser)
            .WithMany()
            .HasForeignKey(c => c.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TicketId);
    }
}
```

### AttachmentConfiguration

```csharp
public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(a => a.Ticket)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.UploadedByUser)
            .WithMany()
            .HasForeignKey(a => a.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.TicketId);
    }
}
```

### ProductConfiguration, DepartmentConfiguration, AuditLogConfiguration, TicketNumberCounterConfiguration

Аналогичны по структуре. `TicketNumberCounter`:
```csharp
public class TicketNumberCounterConfiguration : IEntityTypeConfiguration<TicketNumberCounter>
{
    public void Configure(EntityTypeBuilder<TicketNumberCounter> builder)
    {
        builder.HasKey(c => c.YearMonth);
        builder.Property(c => c.YearMonth).HasMaxLength(7);
        builder.Property(c => c.LastNumber).HasDefaultValue(0);
    }
}
```

---

## 7.2. Порядок создания миграций

```bash
# В корне решения (или в папке SUTP)
dotnet ef migrations add InitialCreate \
    --project TechnicalSupportService.Data \
    --startup-project TechnicalSupportService.SUTP

dotnet ef database update \
    --project TechnicalSupportService.Data \
    --startup-project TechnicalSupportService.SUTP
```

### Рекомендуемый порядок миграций

| # | Миграция | Содержание |
|---|----------|------------|
| 1 | `InitialCreate` | Все таблицы, индексы, FK |
| 2 | `SeedRoles` | Вставка начальных ролей (через Seed, не миграцию) |
| 3 | `SeedProducts` | Начальный набор продуктов (опционально, через Seed) |

> Роли и начальные данные — через `SeedData.cs`, а не через SQL-миграции. Это позволяет переиспользовать логику при повторном развёртывании.

---

## 7.3. Seed-данные

### SeedData.cs — полная реализация

```csharp
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider sp)
    {
        // 1. Роли
        await SeedRolesAsync(sp);

        // 2. Admin-пользователь
        await SeedAdminUserAsync(sp);

        // 3. Начальные продукты
        await SeedProductsAsync(sp);

        // 4. Начальные отделы
        await SeedDepartmentsAsync(sp);
    }

    private static async Task SeedRolesAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        string[] roles = { "Admin", "Engineer", "Manager", "Applicant" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider sp)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        const string email = "admin@company.com";
        if (await userManager.FindByEmailAsync(email) != null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Администратор",
            IsActive = true,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Admin@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "Admin");
    }

    private static async Task SeedProductsAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await db.Products.AnyAsync()) return;

        db.Products.AddRange(
            new Product { Name = "CRM v3.2", ProductType = ProductType.Software, CurrentVersion = "3.2.1" },
            new Product { Name = "Контроллер Т-100", ProductType = ProductType.Hardware, CurrentVersion = "2.0" },
            new Product { Name = "Встраиваемый модуль M1", ProductType = ProductType.Embedded, CurrentVersion = "1.5.3" }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedDepartmentsAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await db.Departments.AnyAsync()) return;

        db.Departments.AddRange(
            new Department { Name = "IT-отдел" },
            new Department { Name = "Отдел разработки" },
            new Department { Name = "Отдел продаж" },
            new Department { Name = "Бухгалтерия" }
        );
        await db.SaveChangesAsync();
    }
}
```

---

## 7.4. Индексы (сводка из §8.1)

| Таблица | Индекс | Тип | Поля |
|---------|--------|-----|------|
| `Tickets` | `IX_Ticket_Number` | UNIQUE | `Number` |
| `Tickets` | `IX_Ticket_CreatedAt` | | `CreatedAt DESC` |
| `Tickets` | `IX_Ticket_Status` | | `Status` |
| `Tickets` | `IX_Ticket_AssignedToUserId` | | `AssignedToUserId` |
| `Tickets` | `IX_Ticket_CreatedByUserId` | | `CreatedByUserId` |
| `Tickets` | `IX_Ticket_ProductId` | | `ProductId` |
| `TicketHistory` | `IX_TicketHistory_TicketId_ChangedAt` | | `(TicketId, ChangedAt DESC)` |
| `Attachments` | `IX_Attachments_TicketId` | | `TicketId` |
| `Comments` | `IX_Comments_TicketId` | | `TicketId` |
| `AuditLog` | `IX_AuditLog_UserId_CreatedAt` | | `(UserId, CreatedAt DESC)` |
| `AuditLog` | `IX_AuditLog_EntityName_EntityId` | | `(EntityName, EntityId)` |
