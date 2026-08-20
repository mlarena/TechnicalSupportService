# Шаг 6. Бизнес-логика сервисов

## 6.1. Архитектура сервисного слоя

```
Controllers (SUTP)  →  IService (Core)  →  Service Implementation (SUTP)  →  DbContext (Data)
         │                     │                        │
         │              DTO / ViewModel          Entity маппинг
         │                     │                   (ручной или AutoMapper)
         └─────────────────────┴───────────────────────┘
```

**Принцип:** Контроллер не работает с DbContext напрямую. Вся логика — в сервисах.

---

## 6.2. Интерфейсы сервисов (Core)

### ITicketService

```csharp
// TechnicalSupportService.Core/Interfaces/ITicketService.cs
public interface ITicketService
{
    Task<TicketDto> GetByIdAsync(Guid id, Guid currentUserId, string currentRole);
    Task<PagedResult<TicketListItemDto>> GetListAsync(TicketFilterDto filter, Guid currentUserId, string currentRole);
    Task<TicketDto> CreateAsync(TicketCreateDto dto, Guid currentUserId);
    Task<TicketDto> UpdateAsync(Guid id, TicketUpdateDto dto, Guid currentUserId);
    Task ChangeStatusAsync(Guid id, TicketStatus newStatus, Guid currentUserId);
    Task AssignAsync(Guid id, Guid? assigneeId, Guid currentUserId);
    Task CloseAsync(Guid id, string? resolution, Guid currentUserId);
    Task ReopenAsync(Guid id, string reason, Guid currentUserId);
    Task DeleteAsync(Guid id, Guid currentUserId);
    Task<byte[]> ExportToExcelAsync(TicketFilterDto filter, Guid currentUserId, string currentRole);
}
```

### ICommentService

```csharp
public interface ICommentService
{
    Task<List<CommentDto>> GetByTicketAsync(Guid ticketId, Guid currentUserId, string currentRole);
    Task<CommentDto> AddAsync(Guid ticketId, CommentCreateDto dto, Guid currentUserId);
    Task<CommentDto> EditAsync(Guid commentId, string newContent, Guid currentUserId);
    Task DeleteAsync(Guid commentId, Guid currentUserId);
}
```

### IAttachmentService

```csharp
public interface IAttachmentService
{
    Task<List<AttachmentDto>> GetByTicketAsync(Guid ticketId);
    Task<AttachmentDto> UploadAsync(Guid ticketId, IFormFile file, Guid currentUserId);
    Task<AttachmentDto> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id, Guid currentUserId);
    Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(Guid id);
}
```

### IFileStorageService

```csharp
public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string originalFileName, Guid ticketId);
    Task<Stream> ReadAsync(string filePath);
    Task DeleteAsync(string filePath);
}
```

### INumberGeneratorService

```csharp
public interface INumberGeneratorService
{
    Task<string> GenerateNextNumberAsync();
}
```

### IAuditService

```csharp
public interface IAuditService
{
    Task LogAsync(string action, Guid userId, string? entityName = null,
                  Guid? entityId = null, string? details = null,
                  string? ipAddress = null, string? userAgent = null);
}
```

### IDashboardService

```csharp
public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid currentUserId, string currentRole);
}
```

### IProductService / IDepartmentService / IUserService

```csharp
public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(bool includeInactive = false);
    Task<ProductDto> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(ProductCreateDto dto);
    Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto dto);
    Task DeleteAsync(Guid id); // soft delete
}

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync(bool includeInactive = false);
    Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto);
    Task<DepartmentDto> UpdateAsync(Guid id, DepartmentUpdateDto dto);
}

public interface IUserService
{
    Task<PagedResult<UserDto>> GetListAsync(UserFilterDto filter);
    Task<UserDto> GetByIdAsync(Guid id);
    Task<(bool success, IEnumerable<string> errors)> CreateAsync(UserCreateDto dto);
    Task<(bool success, IEnumerable<string> errors)> UpdateAsync(Guid id, UserUpdateDto dto);
    Task BlockAsync(Guid id, bool block);
    Task DeleteAsync(Guid id);
    Task<List<UserDto>> GetEngineersAsync(); // для dropdown назначения
}
```

---

## 6.3. Ключевые DTO (Core)

```csharp
// TicketDto — полная информация о заявке
public record TicketDto
{
    public Guid Id { get; init; }
    public string Number { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string ProductName { get; init; } = "";
    public string ProductType { get; init; } = "";
    public string? Version { get; init; }
    public string Priority { get; init; } = "";
    public string Status { get; init; } = "";
    public string Category { get; init; } = "";
    public string? Impact { get; init; }
    public string Source { get; init; } = "";
    public string? AssignedToUserName { get; init; }
    public string CreatedByUserName { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public string UpdatedByUserName { get; init; } = "";
    public DateTime UpdatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public string? Resolution { get; init; }
    public int? TimeSpentMinutes { get; init; }
    public Guid? ParentTicketId { get; init; }
    public List<CommentDto> Comments { get; init; } = new();
    public List<AttachmentDto> Attachments { get; init; } = new();
    public List<TicketHistoryDto> History { get; init; } = new();
}

// TicketListItemDto — для списка (компактный)
public record TicketListItemDto
{
    public Guid Id { get; init; }
    public string Number { get; init; } = "";
    public string Title { get; init; } = "";
    public string Status { get; init; } = "";
    public string Priority { get; init; } = "";
    public string ProductName { get; init; } = "";
    public string? AssignedToUserName { get; init; }
    public DateTime CreatedAt { get; init; }
}

// TicketCreateDto — для создания
public record TicketCreateDto
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = "";

    [Required]
    public string Description { get; init; } = "";

    [Required]
    public Guid ProductId { get; init; }

    [MaxLength(20)]
    public string? Version { get; init; }

    [Required]
    public Priority Priority { get; init; }

    [Required]
    public Category Category { get; init; }

    public Impact? Impact { get; init; }

    [Required]
    public Source Source { get; init; }

    public Guid? AssignedToUserId { get; init; }
}

// TicketFilterDto — фильтры для списка
public record TicketFilterDto
{
    public TicketStatus? Status { get; init; }
    public Priority? Priority { get; init; }
    public Category? Category { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? Search { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "CreatedAt";
    public string SortDir { get; init; } = "desc";
}

// PagedResult<T> — обёртка для пагинации
public record PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

---

## 6.4. Реализация TicketService — ключевые методы

### Создание заявки

```csharp
public async Task<TicketDto> CreateAsync(TicketCreateDto dto, Guid currentUserId)
{
    // 1. Генерация номера
    var number = await _numberGenerator.GenerateNextNumberAsync();

    // 2. Создание сущности
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
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    // 3. Если назначен исполнитель — статус = Assigned
    if (dto.AssignedToUserId.HasValue)
    {
        ticket.Status = TicketStatus.Assigned;
    }

    _db.Tickets.Add(ticket);

    // 4. Запись в историю
    _db.TicketHistories.Add(new TicketHistory
    {
        Id = Guid.NewGuid(),
        TicketId = ticket.Id,
        ChangedByUserId = currentUserId,
        ChangedAt = DateTime.UtcNow,
        ChangeType = ChangeType.Creation,
        NewValue = $"Заявка создана: {ticket.Title}"
    });

    // 5. Аудит
    await _audit.LogAsync("Ticket.Create", currentUserId, "Ticket", ticket.Id);

    await _db.SaveChangesAsync();

    return MapToDto(ticket);
}
```

### Изменение статуса

```csharp
public async Task ChangeStatusAsync(Guid id, TicketStatus newStatus, Guid currentUserId)
{
    var ticket = await _db.Tickets.FindAsync(id)
        ?? throw new NotFoundException("Заявка не найдена");

    var currentRole = await GetUserRoleAsync(currentUserId);

    // Валидация перехода
    if (!IsValidTransition(ticket.Status, newStatus, currentRole))
        throw new BusinessRuleException(
            $"Переход {ticket.Status} → {newStatus} недопустим для роли {currentRole}");

    var oldStatus = ticket.Status;
    ticket.Status = newStatus;
    ticket.UpdatedAt = DateTime.UtcNow;
    ticket.UpdatedByUserId = currentUserId;

    // Дополнительные действия при переходе
    if (newStatus == TicketStatus.Closed)
    {
        ticket.ClosedAt = DateTime.UtcNow;
    }
    if (newStatus == TicketStatus.Assigned && !ticket.AssignedToUserId.HasValue)
    {
        throw new BusinessRuleException("Нельзя перевести в Assigned без исполнителя");
    }

    // История
    _db.TicketHistories.Add(new TicketHistory
    {
        Id = Guid.NewGuid(),
        TicketId = id,
        ChangedByUserId = currentUserId,
        ChangedAt = DateTime.UtcNow,
        FieldName = "Status",
        OldValue = oldStatus.ToString(),
        NewValue = newStatus.ToString(),
        ChangeType = ChangeType.StatusChange
    });

    await _audit.LogAsync("Ticket.StatusChange", currentUserId, "Ticket", id,
        $"Status: {oldStatus} → {newStatus}");

    await _db.SaveChangesAsync();
}
```

### Проверка допустимости перехода

```csharp
private bool IsValidTransition(TicketStatus from, TicketStatus to, string role)
{
    return (from, to, role) switch
    {
        (TicketStatus.New, TicketStatus.Assigned, "Admin" or "Manager") => true,
        (TicketStatus.Assigned, TicketStatus.InProgress, "Engineer") => true,
        (TicketStatus.InProgress, TicketStatus.Resolved, "Engineer") => true,
        (TicketStatus.Resolved, TicketStatus.Closed, "Admin" or "Manager" or "Applicant") => true,
        (TicketStatus.Resolved, TicketStatus.Reopened, "Admin" or "Manager" or "Applicant") => true,
        (TicketStatus.Closed, TicketStatus.Reopened, "Admin" or "Manager" or "Applicant") => true,
        (_, TicketStatus.Closed, "Admin" or "Manager") => true, // принудительное закрытие
        _ => false
    };
}
```

---

## 6.5. Реализация NumberGeneratorService

```csharp
public class NumberGeneratorService : INumberGeneratorService
{
    private readonly ApplicationDbContext _db;

    public async Task<string> GenerateNextNumberAsync()
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyy-MM");

        // SELECT ... FOR UPDATE — блокировка строки
        var counter = await _db.TicketNumberCounters
            .FromSqlRaw(
                "SELECT * FROM \"TicketNumberCounters\" WHERE \"YearMonth\" = {0} FOR UPDATE",
                yearMonth)
            .FirstOrDefaultAsync();

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

        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        return $"{year:0000}_{month:00}_{counter.LastNumber:D3}";
    }
}
```

---

## 6.6. Реализация FileStorageService (локальный диск)

```csharp
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

    public async Task<Stream> ReadAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
            throw new NotFoundException("Файл не найден");

        return await Task.FromResult<Stream>(File.OpenRead(fullPath));
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

---

## 6.7. Сервисные исключения

```csharp
// TechnicalSupportService.Core/Exceptions/
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
```

### Middleware для обработки исключений

```csharp
// TechnicalSupportService.SUTP/Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (ForbiddenException)
        {
            context.Response.StatusCode = 403;
        }
        catch (BusinessRuleException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }
}
```
