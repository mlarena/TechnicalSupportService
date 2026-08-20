# Шаг 2. Сущности, enum'ы и DbContext — точный C# код

> Каждый блок кода — отдельный файл. Путь указан в заголовке блока.
> Namespace соответствует пути от корня проекта.

---

## 2.1. Enum'ы (Core/Enums/)

### Core/Enums/ProductType.cs

```csharp
namespace TechnicalSupportService.Core.Enums;

public enum ProductType
{
    Software,
    Hardware,
    Embedded
}
```

### Core/Enums/Priority.cs

```csharp
namespace TechnicalSupportService.Core.Enums;

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}
```

### Core/Enums/TicketStatus.cs

```csharp
namespace TechnicalSupportService.Core.Enums;

public enum TicketStatus
{
    New,
    Assigned,
    InProgress,
    Resolved,
    Closed,
    Reopened
}
```

### Core/Enums/Category.cs

```csharp
namespace TechnicalSupportService.Core.Enums;

public enum Category
{
    Bug,
    Feature,
    Support,
    Incident
}
```

### Core/Enums/Impact.cs

```csharp
namespace TechnicalSupportService.Core.Enums;

public enum Impact
{
    Individual,
    Team,
    Department,
    Company
}
```

### Core/Enums/Source.cs

```csharp
namespace TechnicalSupportService.Core.Enums;

public enum Source
{
    Email,
    Phone,
    Portal,
    Internal
}
```

### Core/Enums/ChangeType.cs

```csharp
namespace TechnicalSupportService.Core.Enums;

public enum ChangeType
{
    Creation,
    Update,
    StatusChange,
    Assignment,
    Comment,
    FileAttach
}
```

---

## 2.2. Сущности (Data/Entities/)

### Data/Entities/ApplicationUser.cs

```csharp
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicalSupportService.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public Guid? DepartmentId { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department? Department { get; set; }
}
```

### Data/Entities/ApplicationRole.cs

```csharp
using Microsoft.AspNetCore.Identity;

namespace TechnicalSupportService.Data.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
```

### Data/Entities/Department.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace TechnicalSupportService.Data.Entities;

public class Department
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
```

### Data/Entities/Product.cs

```csharp
using System.ComponentModel.DataAnnotations;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Data.Entities;

public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ProductType ProductType { get; set; }

    [MaxLength(20)]
    public string? CurrentVersion { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
```

### Data/Entities/Ticket.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Data.Entities;

public class Ticket
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public Guid ProductId { get; set; }

    [MaxLength(20)]
    public string? Version { get; set; }

    public Priority Priority { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.New;

    public Category Category { get; set; }

    public Impact? Impact { get; set; }

    public Source Source { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UpdatedByUserId { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAt { get; set; }

    public string? Resolution { get; set; }

    public int? TimeSpentMinutes { get; set; }

    public Guid? ParentTicketId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }

    // Навигационные свойства
    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey(nameof(AssignedToUserId))]
    public virtual ApplicationUser? AssignedToUser { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public virtual ApplicationUser CreatedByUser { get; set; } = null!;

    [ForeignKey(nameof(UpdatedByUserId))]
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;

    [ForeignKey(nameof(ParentTicketId))]
    public virtual Ticket? ParentTicket { get; set; }

    [ForeignKey(nameof(DeletedByUserId))]
    public virtual ApplicationUser? DeletedByUser { get; set; }

    public virtual ICollection<Ticket> ChildTickets { get; set; } = new List<Ticket>();
    public virtual ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
```

### Data/Entities/TicketHistory.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Data.Entities;

public class TicketHistory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }

    public Guid ChangedByUserId { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public ChangeType ChangeType { get; set; }

    public Guid? CommentId { get; set; }

    public Guid? AttachmentId { get; set; }

    // Навигационные свойства
    [ForeignKey(nameof(TicketId))]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey(nameof(ChangedByUserId))]
    public virtual ApplicationUser ChangedByUser { get; set; } = null!;

    [ForeignKey(nameof(CommentId))]
    public virtual Comment? Comment { get; set; }

    [ForeignKey(nameof(AttachmentId))]
    public virtual Attachment? Attachment { get; set; }
}
```

### Data/Entities/Comment.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicalSupportService.Data.Entities;

public class Comment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }

    public Guid AuthorUserId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;

    public bool IsEdited { get; set; } = false;

    public DateTime? EditedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    // Навигационные свойства
    [ForeignKey(nameof(TicketId))]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey(nameof(AuthorUserId))]
    public virtual ApplicationUser AuthorUser { get; set; } = null!;
}
```

### Data/Entities/Attachment.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicalSupportService.Data.Entities;

public class Attachment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    [Required]
    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;

    public Guid UploadedByUserId { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    // Навигационные свойства
    [ForeignKey(nameof(TicketId))]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey(nameof(UploadedByUserId))]
    public virtual ApplicationUser UploadedByUser { get; set; } = null!;
}
```

### Data/Entities/TicketNumberCounter.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace TechnicalSupportService.Data.Entities;

public class TicketNumberCounter
{
    [Key]
    [MaxLength(7)]
    public string YearMonth { get; set; } = string.Empty;

    public int LastNumber { get; set; } = 0;
}
```

### Data/Entities/AuditLog.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechnicalSupportService.Data.Entities;

public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityName { get; set; }

    public Guid? EntityId { get; set; }

    public string? Details { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}
```

---

## 2.3. DbContext (Data/Context/)

### Data/Context/ApplicationDbContext.cs

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Context;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
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
