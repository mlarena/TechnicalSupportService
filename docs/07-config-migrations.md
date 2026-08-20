# Шаг 7. EF Core Fluent API-конфигурации

> Все файлы в `TechnicalSupportService.Data/Configurations/`.

---

## 7.1. TicketConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Number).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.Number).IsUnique();

        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.Version).HasMaxLength(20);
        builder.Property(t => t.Resolution);

        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Impact).HasConversion<string?>().HasMaxLength(20);
        builder.Property(t => t.Source).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(t => t.Product).WithMany(p => p.Tickets)
            .HasForeignKey(t => t.ProductId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreatedByUser).WithMany()
            .HasForeignKey(t => t.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedToUser).WithMany()
            .HasForeignKey(t => t.AssignedToUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.UpdatedByUser).WithMany()
            .HasForeignKey(t => t.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ParentTicket).WithMany(t => t.ChildTickets)
            .HasForeignKey(t => t.ParentTicketId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.DeletedByUser).WithMany()
            .HasForeignKey(t => t.DeletedByUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => t.CreatedByUserId);
        builder.HasIndex(t => t.ProductId);
    }
}
```

---

## 7.2. TicketHistoryConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.FieldName).HasMaxLength(50);
        builder.Property(h => h.ChangeType).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(h => h.Ticket).WithMany(t => t.History)
            .HasForeignKey(h => h.TicketId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedByUser).WithMany()
            .HasForeignKey(h => h.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Comment).WithMany()
            .HasForeignKey(h => h.CommentId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.Attachment).WithMany()
            .HasForeignKey(h => h.AttachmentId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(h => new { h.TicketId, h.ChangedAt });
    }
}
```

---

## 7.3. CommentConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).IsRequired();

        builder.HasOne(c => c.Ticket).WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AuthorUser).WithMany()
            .HasForeignKey(c => c.AuthorUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TicketId);
    }
}
```

---

## 7.4. AttachmentConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.StoredFileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(a => a.MimeType).IsRequired().HasMaxLength(100);

        builder.HasOne(a => a.Ticket).WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TicketId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.UploadedByUser).WithMany()
            .HasForeignKey(a => a.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.TicketId);
    }
}
```

---

## 7.5. ProductConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ProductType).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CurrentVersion).HasMaxLength(20);
        builder.Property(p => p.Description).HasMaxLength(500);
    }
}
```

---

## 7.6. DepartmentConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(500);
    }
}
```

---

## 7.7. TicketNumberCounterConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

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

## 7.8. AuditLogConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityName).HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);

        builder.HasOne(a => a.User).WithMany()
            .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.UserId, a.CreatedAt });
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
    }
}
```

---

## 7.9. Команды миграций

```powershell
# Создание миграции (из корня решения)
dotnet ef migrations add InitialCreate `
    --project TechnicalSupportService.Data `
    --startup-project TechnicalSupportService.SUTP

# Применение миграции
dotnet ef database update `
    --project TechnicalSupportService.Data `
    --startup-project TechnicalSupportService.SUTP
```
