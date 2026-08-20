# Шаг 6. Exceptions, DTO, Interfaces и Services — точный C# код

---

## 6.1. Exceptions (Core/Exceptions/)

### Core/Exceptions/NotFoundException.cs

```csharp
namespace TechnicalSupportService.Core.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
```

### Core/Exceptions/BusinessRuleException.cs

```csharp
namespace TechnicalSupportService.Core.Exceptions;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
```

### Core/Exceptions/ForbiddenException.cs

```csharp
namespace TechnicalSupportService.Core.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
```

---

## 6.2. DTO (Core/DTOs/)

### Core/DTOs/PagedResult.cs

```csharp
namespace TechnicalSupportService.Core.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

### Core/DTOs/TicketDto.cs

```csharp
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class TicketDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string ProductType { get; set; } = "";
    public string? Version { get; set; }
    public Priority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public Category Category { get; set; }
    public Impact? Impact { get; set; }
    public Source Source { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string CreatedByUserName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Resolution { get; set; }
    public int? TimeSpentMinutes { get; set; }
}
```

### Core/DTOs/TicketListItemDto.cs

```csharp
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class TicketListItemDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public TicketStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string ProductName { get; set; } = "";
    public string? AssignedToUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Core/DTOs/TicketCreateDto.cs

```csharp
using System.ComponentModel.DataAnnotations;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class TicketCreateDto
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Описание обязательно")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Выберите продукт")]
    public Guid ProductId { get; set; }

    [MaxLength(20)]
    public string? Version { get; set; }

    [Required(ErrorMessage = "Выберите приоритет")]
    public Priority Priority { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    public Category Category { get; set; }

    public Impact? Impact { get; set; }

    [Required(ErrorMessage = "Выберите источник")]
    public Source Source { get; set; }

    public Guid? AssignedToUserId { get; set; }
}
```

### Core/DTOs/TicketUpdateDto.cs

```csharp
using System.ComponentModel.DataAnnotations;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class TicketUpdateDto
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Описание обязательно")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Выберите продукт")]
    public Guid ProductId { get; set; }

    [MaxLength(20)]
    public string? Version { get; set; }

    public Priority Priority { get; set; }
    public Category Category { get; set; }
    public Impact? Impact { get; set; }
}
```

### Core/DTOs/TicketFilterDto.cs

```csharp
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class TicketFilterDto
{
    public TicketStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public Category? Category { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Search { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortDir { get; set; } = "desc";
}
```

### Core/DTOs/TicketHistoryDto.cs

```csharp
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class TicketHistoryDto
{
    public Guid Id { get; set; }
    public string ChangedByName { get; set; } = "";
    public DateTime ChangedAt { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public ChangeType ChangeType { get; set; }
}
```

### Core/DTOs/CommentDto.cs

```csharp
namespace TechnicalSupportService.Core.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = "";
    public string Content { get; set; } = "";
    public bool IsInternal { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Core/DTOs/CommentCreateDto.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace TechnicalSupportService.Core.DTOs;

public class CommentCreateDto
{
    [Required(ErrorMessage = "Текст комментария обязателен")]
    public string Content { get; set; } = "";

    public bool IsInternal { get; set; } = false;
}
```

### Core/DTOs/AttachmentDto.cs

```csharp
namespace TechnicalSupportService.Core.DTOs;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = "";
    public string UploadedByName { get; set; } = "";
    public DateTime UploadedAt { get; set; }
}
```

### Core/DTOs/ProductDto.cs

```csharp
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public ProductType ProductType { get; set; }
    public string? CurrentVersion { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class ProductCreateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = "";
    public ProductType ProductType { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? CurrentVersion { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Description { get; set; }
}
```

### Core/DTOs/DepartmentDto.cs

```csharp
namespace TechnicalSupportService.Core.DTOs;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class DepartmentCreateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = "";
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Description { get; set; }
}
```

### Core/DTOs/UserDto.cs

```csharp
namespace TechnicalSupportService.Core.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Position { get; set; }
    public string? DepartmentName { get; set; }
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
}

public class UserFilterDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class UserCreateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string FullName { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MinLength(8)]
    public string Password { get; set; } = "";
    public string? Position { get; set; }
    public Guid? DepartmentId { get; set; }
    [System.ComponentModel.DataAnnotations.Required]
    public string Role { get; set; } = "";
}

public class UserUpdateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string FullName { get; set; } = "";
    public string? Position { get; set; }
    public Guid? DepartmentId { get; set; }
    public string Role { get; set; } = "";
}
```

### Core/DTOs/DashboardDto.cs

```csharp
namespace TechnicalSupportService.Core.DTOs;

public class DashboardDto
{
    public Dictionary<string, int> TicketsByStatus { get; set; } = new();
    public Dictionary<string, int> TicketsByPriority { get; set; } = new();
    public List<TicketListItemDto> RecentTickets { get; set; } = new();
    public int CriticalCount { get; set; }
    public int TotalOpen { get; set; }
}
```

---

## 6.3. Interfaces (Core/Interfaces/)

### Core/Interfaces/ITicketService.cs

```csharp
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.Interfaces;

public interface ITicketService
{
    Task<TicketDto?> GetByIdAsync(Guid id);
    Task<PagedResult<TicketListItemDto>> GetListAsync(TicketFilterDto filter, Guid currentUserId, string currentRole);
    Task<TicketDto> CreateAsync(TicketCreateDto dto, Guid currentUserId);
    Task<TicketDto> UpdateAsync(Guid id, TicketUpdateDto dto, Guid currentUserId);
    Task ChangeStatusAsync(Guid id, TicketStatus newStatus, string? resolution, Guid currentUserId);
    Task AssignAsync(Guid id, Guid? assigneeId, Guid currentUserId);
    Task CloseAsync(Guid id, string? resolution, Guid currentUserId);
    Task ReopenAsync(Guid id, Guid currentUserId);
    Task DeleteAsync(Guid id, Guid currentUserId);
    Task<List<TicketHistoryDto>> GetHistoryAsync(Guid ticketId);
}
```

### Core/Interfaces/ICommentService.cs

```csharp
using TechnicalSupportService.Core.DTOs;

namespace TechnicalSupportService.Core.Interfaces;

public interface ICommentService
{
    Task<List<CommentDto>> GetByTicketAsync(Guid ticketId, string currentRole);
    Task<CommentDto> AddAsync(Guid ticketId, CommentCreateDto dto, Guid currentUserId);
    Task<CommentDto> EditAsync(Guid commentId, string newContent, Guid currentUserId);
    Task DeleteAsync(Guid commentId, Guid currentUserId);
}
```

### Core/Interfaces/IAttachmentService.cs

```csharp
using TechnicalSupportService.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace TechnicalSupportService.Core.Interfaces;

public interface IAttachmentService
{
    Task<List<AttachmentDto>> GetByTicketAsync(Guid ticketId);
    Task<AttachmentDto> UploadAsync(Guid ticketId, IFormFile file, Guid currentUserId);
    Task<AttachmentDto?> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id, Guid currentUserId);
    Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(Guid id);
}
```

### Core/Interfaces/IFileStorageService.cs

```csharp
namespace TechnicalSupportService.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string originalFileName, Guid ticketId);
    Task<Stream> ReadAsync(string filePath);
    Task DeleteAsync(string filePath);
}
```

### Core/Interfaces/INumberGeneratorService.cs

```csharp
namespace TechnicalSupportService.Core.Interfaces;

public interface INumberGeneratorService
{
    Task<string> GenerateNextNumberAsync();
}
```

### Core/Interfaces/IAuditService.cs

```csharp
namespace TechnicalSupportService.Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, Guid userId, string? entityName = null,
        Guid? entityId = null, string? details = null,
        string? ipAddress = null, string? userAgent = null);
}
```

### Core/Interfaces/IDashboardService.cs

```csharp
using TechnicalSupportService.Core.DTOs;

namespace TechnicalSupportService.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid currentUserId, string currentRole);
}
```

### Core/Interfaces/IProductService.cs

```csharp
using TechnicalSupportService.Core.DTOs;

namespace TechnicalSupportService.Core.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(bool includeInactive = false);
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(ProductCreateDto dto);
    Task<ProductDto> UpdateAsync(Guid id, ProductCreateDto dto);
}
```

### Core/Interfaces/IDepartmentService.cs

```csharp
using TechnicalSupportService.Core.DTOs;

namespace TechnicalSupportService.Core.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync(bool includeInactive = false);
    Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto);
    Task<DepartmentDto> UpdateAsync(Guid id, DepartmentCreateDto dto);
}
```

### Core/Interfaces/IUserService.cs

```csharp
using TechnicalSupportService.Core.DTOs;

namespace TechnicalSupportService.Core.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetListAsync(UserFilterDto filter);
    Task<UserDto?> GetByIdAsync Guid id);
    Task<(bool success, List<string> errors)> CreateAsync(UserCreateDto dto);
    Task<(bool success, List<string> errors)> UpdateAsync(Guid id, UserUpdateDto dto);
    Task BlockAsync(Guid id, bool block);
    Task DeleteAsync(Guid id);
    Task<List<UserDto>> GetEngineersAsync();
}
```

---

## 6.4. Services (SUTP/Services/) — ключевые реализации

> Полные реализации. Все сервисы принимают `ApplicationDbContext` и (опционально) `IHttpContextAccessor` через конструктор.

### SUTP/Services/NumberGeneratorService.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class NumberGeneratorService : INumberGeneratorService
{
    private readonly ApplicationDbContext _db;

    public NumberGeneratorService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateNextNumberAsync()
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyy-MM");

        var counter = await _db.TicketNumberCounters
            .FirstOrDefaultAsync(c => c.YearMonth == yearMonth);

        if (counter == null)
        {
            counter = new TicketNumberCounter
            {
                YearMonth = yearMonth,
                LastNumber = 1
            };
            _db.TicketNumberCounters.Add(counter);
        }
        else
        {
            counter.LastNumber++;
        }

        await _db.SaveChangesAsync();

        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        return $"{year:0000}_{month:00}_{counter.LastNumber:D3}";
    }
}
```

### SUTP/Services/TicketService.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _db;
    private readonly INumberGeneratorService _numberGen;
    private readonly IAuditService _audit;

    public TicketService(ApplicationDbContext db, INumberGeneratorService numberGen, IAuditService audit)
    {
        _db = db;
        _numberGen = numberGen;
        _audit = audit;
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id)
    {
        var ticket = await _db.Tickets
            .Include(t => t.Product)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        return ticket == null ? null : MapToDto(ticket);
    }

    public async Task<PagedResult<TicketListItemDto>> GetListAsync(
        TicketFilterDto filter, Guid currentUserId, string currentRole)
    {
        var query = _db.Tickets
            .Include(t => t.Product)
            .Include(t => t.AssignedToUser)
            .Where(t => !t.IsDeleted);

        // Заявитель видит только свои
        if (currentRole == Roles.Applicant)
            query = query.Where(t => t.CreatedByUserId == currentUserId);

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);
        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);
        if (filter.Category.HasValue)
            query = query.Where(t => t.Category == filter.Category.Value);
        if (filter.ProductId.HasValue)
            query = query.Where(t => t.ProductId == filter.ProductId.Value);
        if (filter.AssignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(t =>
                t.Number.Contains(filter.Search) ||
                t.Title.Contains(filter.Search) ||
                t.Description.Contains(filter.Search));
        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo.Value);

        var totalCount = await query.CountAsync();

        query = filter.SortDir == "asc"
            ? query.OrderBy(t => t.CreatedAt)
            : query.OrderByDescending(t => t.CreatedAt);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new TicketListItemDto
            {
                Id = t.Id,
                Number = t.Number,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                ProductName = t.Product.Name,
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<TicketListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<TicketDto> CreateAsync(TicketCreateDto dto, Guid currentUserId)
    {
        var number = await _numberGen.GenerateNextNumberAsync();

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Number = number,
            Title = dto.Title,
            Description = dto.Description,
            ProductId = dto.ProductId,
            Version = dto.Version,
            Priority = dto.Priority,
            Status = TicketStatus.New,
            Category = dto.Category,
            Impact = dto.Impact,
            Source = dto.Source,
            AssignedToUserId = dto.AssignedToUserId,
            CreatedByUserId = currentUserId,
            UpdatedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (dto.AssignedToUserId.HasValue)
            ticket.Status = TicketStatus.Assigned;

        _db.Tickets.Add(ticket);

        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticket.Id,
            ChangedByUserId = currentUserId,
            ChangeType = ChangeType.Creation,
            NewValue = $"Создана заявка: {ticket.Title}"
        });

        await _audit.LogAsync("Ticket.Create", currentUserId, "Ticket", ticket.Id);
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(ticket.Id))!;
    }

    public async Task<TicketDto> UpdateAsync(Guid id, TicketUpdateDto dto, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(id)
            ?? throw new NotFoundException("Заявка не найдена");

        var changes = new List<string>();
        if (ticket.Title != dto.Title) { changes.Add($"Title: {ticket.Title} → {dto.Title}"); ticket.Title = dto.Title; }
        if (ticket.Description != dto.Description) { changes.Add($"Description изменено"); ticket.Description = dto.Description; }
        if (ticket.ProductId != dto.ProductId) { changes.Add($"ProductId: {ticket.ProductId} → {dto.ProductId}"); ticket.ProductId = dto.ProductId; }
        if (ticket.Version != dto.Version) { changes.Add($"Version: {ticket.Version} → {dto.Version}"); ticket.Version = dto.Version; }
        if (ticket.Priority != dto.Priority) { changes.Add($"Priority: {ticket.Priority} → {dto.Priority}"); ticket.Priority = dto.Priority; }
        if (ticket.Category != dto.Category) { changes.Add($"Category: {ticket.Category} → {dto.Category}"); ticket.Category = dto.Category; }
        if (ticket.Impact != dto.Impact) { changes.Add($"Impact: {ticket.Impact} → {dto.Impact}"); ticket.Impact = dto.Impact; }

        ticket.UpdatedByUserId = currentUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        foreach (var change in changes)
        {
            _db.TicketHistories.Add(new TicketHistory
            {
                TicketId = id,
                ChangedByUserId = currentUserId,
                ChangeType = ChangeType.Update,
                NewValue = change
            });
        }

        await _audit.LogAsync("Ticket.Update", currentUserId, "Ticket", id);
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task ChangeStatusAsync(Guid id, TicketStatus newStatus, string? resolution, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(id)
            ?? throw new NotFoundException("Заявка не найдена");

        var userRole = await GetUserRoleAsync(currentUserId);

        if (!IsValidTransition(ticket.Status, newStatus, userRole))
            throw new BusinessRuleException($"Переход {ticket.Status} → {newStatus} недопустим для роли {userRole}");

        var oldStatus = ticket.Status;
        ticket.Status = newStatus;
        ticket.UpdatedByUserId = currentUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (newStatus == TicketStatus.Resolved && !string.IsNullOrWhiteSpace(resolution))
            ticket.Resolution = resolution;
        if (newStatus == TicketStatus.Closed)
            ticket.ClosedAt = DateTime.UtcNow;

        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = id,
            ChangedByUserId = currentUserId,
            ChangeType = ChangeType.StatusChange,
            FieldName = "Status",
            OldValue = oldStatus.ToString(),
            NewValue = newStatus.ToString()
        });

        await _audit.LogAsync("Ticket.StatusChange", currentUserId, "Ticket", id,
            $"{oldStatus} → {newStatus}");
        await _db.SaveChangesAsync();
    }

    public async Task AssignAsync(Guid id, Guid? assigneeId, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(id)
            ?? throw new NotFoundException("Заявка не найдена");

        var oldAssignee = ticket.AssignedToUserId;
        ticket.AssignedToUserId = assigneeId;
        ticket.UpdatedByUserId = currentUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (assigneeId.HasValue && ticket.Status == TicketStatus.New)
            ticket.Status = TicketStatus.Assigned;

        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = id,
            ChangedByUserId = currentUserId,
            ChangeType = ChangeType.Assignment,
            FieldName = "AssignedToUserId",
            OldValue = oldAssignee?.ToString(),
            NewValue = assigneeId?.ToString()
        });

        await _audit.LogAsync("Ticket.Assign", currentUserId, "Ticket", id);
        await _db.SaveChangesAsync();
    }

    public async Task CloseAsync(Guid id, string? resolution, Guid currentUserId)
    {
        await ChangeStatusAsync(id, TicketStatus.Closed, resolution, currentUserId);
    }

    public async Task ReopenAsync(Guid id, Guid currentUserId)
    {
        await ChangeStatusAsync(id, TicketStatus.Reopened, null, currentUserId);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(id)
            ?? throw new NotFoundException("Заявка не найдена");

        ticket.IsDeleted = true;
        ticket.DeletedAt = DateTime.UtcNow;
        ticket.DeletedByUserId = currentUserId;

        await _audit.LogAsync("Ticket.Delete", currentUserId, "Ticket", id);
        await _db.SaveChangesAsync();
    }

    public async Task<List<TicketHistoryDto>> GetHistoryAsync(Guid ticketId)
    {
        return await _db.TicketHistories
            .Include(h => h.ChangedByUser)
            .Where(h => h.TicketId == ticketId)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new TicketHistoryDto
            {
                Id = h.Id,
                ChangedByName = h.ChangedByUser.FullName,
                ChangedAt = h.ChangedAt,
                FieldName = h.FieldName,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                ChangeType = h.ChangeType
            })
            .ToListAsync();
    }

    private static bool IsValidTransition(TicketStatus from, TicketStatus to, string role)
    {
        return (from, to, role) switch
        {
            (TicketStatus.New, TicketStatus.Assigned, Roles.Admin or Roles.Manager) => true,
            (TicketStatus.Assigned, TicketStatus.InProgress, Roles.Engineer) => true,
            (TicketStatus.InProgress, TicketStatus.Resolved, Roles.Engineer) => true,
            (TicketStatus.Resolved, TicketStatus.Closed, _) => true,
            (TicketStatus.Resolved, TicketStatus.Reopened, Roles.Applicant or Roles.Manager or Roles.Admin) => true,
            (TicketStatus.Closed, TicketStatus.Reopened, Roles.Applicant or Roles.Manager or Roles.Admin) => true,
            (_, TicketStatus.Closed, Roles.Admin or Roles.Manager) => true,
            _ => false
        };
    }

    private async Task<string> GetUserRoleAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Roles.Applicant;

        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync();

        return roles.FirstOrDefault() ?? Roles.Applicant;
    }

    private static TicketDto MapToDto(Ticket t)
    {
        return new TicketDto
        {
            Id = t.Id,
            Number = t.Number,
            Title = t.Title,
            Description = t.Description,
            ProductName = t.Product?.Name ?? "",
            ProductType = t.Product?.ProductType.ToString() ?? "",
            Version = t.Version,
            Priority = t.Priority,
            Status = t.Status,
            Category = t.Category,
            Impact = t.Impact,
            Source = t.Source,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUserName = t.AssignedToUser?.FullName,
            CreatedByUserName = t.CreatedByUser?.FullName ?? "",
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            ClosedAt = t.ClosedAt,
            Resolution = t.Resolution,
            TimeSpentMinutes = t.TimeSpentMinutes
        };
    }
}
```

### SUTP/Services/CommentService.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public CommentService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<CommentDto>> GetByTicketAsync(Guid ticketId, string currentRole)
    {
        var query = _db.Comments
            .Include(c => c.AuthorUser)
            .Where(c => c.TicketId == ticketId && !c.IsDeleted);

        // Заявитель не видит внутренние комментарии
        if (currentRole == Core.Constants.Roles.Applicant)
            query = query.Where(c => !c.IsInternal);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                AuthorName = c.AuthorUser.FullName,
                Content = c.Content,
                IsInternal = c.IsInternal,
                IsEdited = c.IsEdited,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CommentDto> AddAsync(Guid ticketId, CommentCreateDto dto, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId)
            ?? throw new NotFoundException("Заявка не найдена");

        var comment = new Comment
        {
            TicketId = ticketId,
            AuthorUserId = currentUserId,
            Content = dto.Content,
            IsInternal = dto.IsInternal,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);

        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticketId,
            ChangedByUserId = currentUserId,
            ChangeType = ChangeType.Comment,
            CommentId = comment.Id,
            NewValue = dto.Content.Length > 100 ? dto.Content[..100] + "..." : dto.Content
        });

        await _audit.LogAsync("Comment.Add", currentUserId, "Comment", comment.Id);
        await _db.SaveChangesAsync();

        var author = await _db.Users.FindAsync(currentUserId);
        return new CommentDto
        {
            Id = comment.Id,
            AuthorName = author?.FullName ?? "",
            Content = comment.Content,
            IsInternal = comment.IsInternal,
            IsEdited = false,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<CommentDto> EditAsync(Guid commentId, string newContent, Guid currentUserId)
    {
        var comment = await _db.Comments
            .Include(c => c.AuthorUser)
            .FirstOrDefaultAsync(c => c.Id == commentId)
            ?? throw new NotFoundException("Комментарий не найден");

        if (comment.AuthorUserId != currentUserId)
            throw new ForbiddenException("Можно редактировать только свои комментарии");

        comment.Content = newContent;
        comment.IsEdited = true;
        comment.EditedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            AuthorName = comment.AuthorUser.FullName,
            Content = comment.Content,
            IsInternal = comment.IsInternal,
            IsEdited = true,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task DeleteAsync(Guid commentId, Guid currentUserId)
    {
        var comment = await _db.Comments.FindAsync(commentId)
            ?? throw new NotFoundException("Комментарий не найден");

        comment.IsDeleted = true;
        await _db.SaveChangesAsync();
    }
}
```

### SUTP/Services/AttachmentService.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class AttachmentService : IAttachmentService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly IAuditService _audit;

    public AttachmentService(ApplicationDbContext db, IFileStorageService fileStorage, IAuditService audit)
    {
        _db = db;
        _fileStorage = fileStorage;
        _audit = audit;
    }

    public async Task<List<AttachmentDto>> GetByTicketAsync(Guid ticketId)
    {
        return await _db.Attachments
            .Include(a => a.UploadedByUser)
            .Where(a => a.TicketId == ticketId && !a.IsDeleted)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileSizeBytes = a.FileSizeBytes,
                MimeType = a.MimeType,
                UploadedByName = a.UploadedByUser.FullName,
                UploadedAt = a.UploadedAt
            })
            .ToListAsync();
    }

    public async Task<AttachmentDto> UploadAsync(Guid ticketId, Microsoft.AspNetCore.Http.IFormFile file, Guid currentUserId)
    {
        using var stream = file.OpenReadStream();
        var filePath = await _fileStorage.SaveAsync(stream, file.FileName, ticketId);

        var attachment = new Attachment
        {
            TicketId = ticketId,
            FileName = file.FileName,
            StoredFileName = Path.GetFileName(filePath),
            FilePath = filePath,
            FileSizeBytes = file.Length,
            MimeType = file.ContentType,
            UploadedByUserId = currentUserId,
            UploadedAt = DateTime.UtcNow
        };

        _db.Attachments.Add(attachment);

        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticketId,
            ChangedByUserId = currentUserId,
            ChangeType = ChangeType.FileAttach,
            AttachmentId = attachment.Id,
            NewValue = file.FileName
        });

        await _audit.LogAsync("File.Upload", currentUserId, "Attachment", attachment.Id);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(currentUserId);
        return new AttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FileSizeBytes = attachment.FileSizeBytes,
            MimeType = attachment.MimeType,
            UploadedByName = user?.FullName ?? "",
            UploadedAt = attachment.UploadedAt
        };
    }

    public async Task<AttachmentDto?> GetByIdAsync(Guid id)
    {
        return await _db.Attachments
            .Include(a => a.UploadedByUser)
            .Where(a => a.Id == id && !a.IsDeleted)
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileSizeBytes = a.FileSizeBytes,
                MimeType = a.MimeType,
                UploadedByName = a.UploadedByUser.FullName,
                UploadedAt = a.UploadedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId)
    {
        var attachment = await _db.Attachments.FindAsync(id)
            ?? throw new NotFoundException("Файл не найден");

        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTime.UtcNow;

        await _fileStorage.DeleteAsync(attachment.FilePath);
        await _audit.LogAsync("File.Delete", currentUserId, "Attachment", id);
        await _db.SaveChangesAsync();
    }

    public async Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(Guid id)
    {
        var attachment = await _db.Attachments.FindAsync(id)
            ?? throw new NotFoundException("Файл не найден");

        if (attachment.IsDeleted)
            throw new NotFoundException("Файл был удалён");

        var stream = await _fileStorage.ReadAsync(attachment.FilePath);
        return (stream, attachment.FileName, attachment.MimeType);
    }
}
```

### SUTP/Services/LocalFileStorageService.cs

```csharp
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private static readonly HashSet<string> AllowedExtensions = new(
        new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".png", ".zip" },
        StringComparer.OrdinalIgnoreCase);

    public LocalFileStorageService(IConfiguration config)
    {
        _basePath = config["FileStorage:LocalPath"]
            ?? throw new InvalidOperationException("FileStorage:LocalPath not configured");
    }

    public async Task<string> SaveAsync(Stream fileStream, string originalFileName, Guid ticketId)
    {
        var ext = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(ext))
            throw new BusinessRuleException($"Формат {ext} не допустим");

        var storedName = $"{Guid.NewGuid()}{ext}";
        var relativePath = Path.Combine(ticketId.ToString(), storedName);
        var fullPath = Path.Combine(_basePath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output);

        return relativePath;
    }

    public Task<Stream> ReadAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
            throw new NotFoundException("Файл не найден на диске");

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
```

### SUTP/Services/AuditService.cs

```csharp
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string action, Guid userId, string? entityName = null,
        Guid? entityId = null, string? details = null,
        string? ipAddress = null, string? userAgent = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
```

### SUTP/Services/ProductService.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;

    public ProductService(ApplicationDbContext db) => _db = db;

    public async Task<List<ProductDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.Products.AsQueryable();
        if (!includeInactive) query = query.Where(p => p.IsActive);

        return await query.OrderBy(p => p.Name).Select(p => new ProductDto
        {
            Id = p.Id, Name = p.Name, ProductType = p.ProductType,
            CurrentVersion = p.CurrentVersion, Description = p.Description, IsActive = p.IsActive
        }).ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        return await _db.Products.Where(p => p.Id == id).Select(p => new ProductDto
        {
            Id = p.Id, Name = p.Name, ProductType = p.ProductType,
            CurrentVersion = p.CurrentVersion, Description = p.Description, IsActive = p.IsActive
        }).FirstOrDefaultAsync();
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name, ProductType = dto.ProductType,
            CurrentVersion = dto.CurrentVersion, Description = dto.Description
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id, Name = product.Name, ProductType = product.ProductType,
            CurrentVersion = product.CurrentVersion, Description = product.Description, IsActive = true
        };
    }

    public async Task<ProductDto> UpdateAsync(Guid id, ProductCreateDto dto)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new NotFoundException("Продукт не найден");

        product.Name = dto.Name;
        product.ProductType = dto.ProductType;
        product.CurrentVersion = dto.CurrentVersion;
        product.Description = dto.Description;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id, Name = product.Name, ProductType = product.ProductType,
            CurrentVersion = product.CurrentVersion, Description = product.Description, IsActive = product.IsActive
        };
    }
}
```

### SUTP/Services/DepartmentService.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _db;

    public DepartmentService(ApplicationDbContext db) => _db = db;

    public async Task<List<DepartmentDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.Departments.AsQueryable();
        if (!includeInactive) query = query.Where(d => d.IsActive);

        return await query.OrderBy(d => d.Name).Select(d => new DepartmentDto
        {
            Id = d.Id, Name = d.Name, Description = d.Description, IsActive = d.IsActive
        }).ToListAsync();
    }

    public async Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto)
    {
        var dept = new Department { Name = dto.Name, Description = dto.Description };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();
        return new DepartmentDto { Id = dept.Id, Name = dept.Name, Description = dept.Description, IsActive = true };
    }

    public async Task<DepartmentDto> UpdateAsync(Guid id, DepartmentCreateDto dto)
    {
        var dept = await _db.Departments.FindAsync(id)
            ?? throw new Core.Exceptions.NotFoundException("Отдел не найден");
        dept.Name = dto.Name;
        dept.Description = dto.Description;
        await _db.SaveChangesAsync();
        return new DepartmentDto { Id = dept.Id, Name = dept.Name, Description = dept.Description, IsActive = dept.IsActive };
    }
}
```

### SUTP/Services/UserService.cs

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<PagedResult<UserDto>> GetListAsync(UserFilterDto filter)
    {
        var query = _db.Users.Include(u => u.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(u => u.FullName.Contains(filter.Search) || u.Email!.Contains(filter.Search));
        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync();

        var users = await query.OrderBy(u => u.FullName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            dtos.Add(new UserDto
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email ?? "",
                Position = u.Position, DepartmentName = u.Department?.Name,
                Role = roles.FirstOrDefault() ?? "", IsActive = u.IsActive
            });
        }

        return new PagedResult<UserDto> { Items = dtos, TotalCount = totalCount, Page = filter.Page, PageSize = filter.PageSize };
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _db.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id, FullName = user.FullName, Email = user.Email ?? "",
            Position = user.Position, DepartmentName = user.Department?.Name,
            Role = roles.FirstOrDefault() ?? "", IsActive = user.IsActive
        };
    }

    public async Task<(bool success, List<string> errors)> CreateAsync(UserCreateDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email, Email = dto.Email, FullName = dto.FullName,
            Position = dto.Position, DepartmentId = dto.DepartmentId,
            IsActive = true, EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description).ToList());

        await _userManager.AddToRoleAsync(user, dto.Role);
        return (true, new List<string>());
    }

    public async Task<(bool success, List<string> errors)> UpdateAsync(Guid id, UserUpdateDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Пользователь не найден");

        user.FullName = dto.FullName;
        user.Position = dto.Position;
        user.DepartmentId = dto.DepartmentId;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description).ToList());

        // Обновление роли
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(dto.Role))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        return (true, new List<string>());
    }

    public async Task BlockAsync(Guid id, bool block)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Пользователь не найден");
        user.IsActive = !block;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Пользователь не найден");
        await _userManager.DeleteAsync(user);
    }

    public async Task<List<UserDto>> GetEngineersAsync()
    {
        var engineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer);
        return engineers.Where(u => u.IsActive).Select(u => new UserDto
        {
            Id = u.Id, FullName = u.FullName, Email = u.Email ?? "",
            Position = u.Position, Role = Roles.Engineer, IsActive = u.IsActive
        }).ToList();
    }
}
```

### SUTP/Services/DashboardService.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;

namespace TechnicalSupportService.SUTP.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db) => _db = db;

    public async Task<DashboardDto> GetDashboardAsync(Guid currentUserId, string currentRole)
    {
        var query = _db.Tickets.Include(t => t.Product).Include(t => t.AssignedToUser)
            .Where(t => !t.IsDeleted).AsQueryable();

        if (currentRole == Roles.Applicant)
            query = query.Where(t => t.CreatedByUserId == currentUserId);

        var ticketsByStatus = await query
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var ticketsByPriority = await query
            .Where(t => t.Status != TicketStatus.Closed)
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count);

        var recentTickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new TicketListItemDto
            {
                Id = t.Id, Number = t.Number, Title = t.Title,
                Status = t.Status, Priority = t.Priority,
                ProductName = t.Product.Name,
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        var criticalCount = await query.CountAsync(t =>
            t.Priority == Priority.Critical && t.Status != TicketStatus.Closed);

        var totalOpen = await query.CountAsync(t =>
            t.Status != TicketStatus.Closed);

        return new DashboardDto
        {
            TicketsByStatus = ticketsByStatus,
            TicketsByPriority = ticketsByPriority,
            RecentTickets = recentTickets,
            CriticalCount = criticalCount,
            TotalOpen = totalOpen
        };
    }
}
```
