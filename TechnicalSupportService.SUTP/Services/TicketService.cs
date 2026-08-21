using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;
using TechnicalSupportService.Core.Helpers;

namespace TechnicalSupportService.SUTP.Services;

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _db;
    private readonly INumberGeneratorService _numberGen;
    private readonly IAuditService _audit;

    public TicketService(ApplicationDbContext db, INumberGeneratorService numberGen, IAuditService audit)
    { _db = db; _numberGen = numberGen; _audit = audit; }

    public async Task<TicketDto?> GetByIdAsync(Guid id)
    {
        var t = await _db.Tickets
            .Include(x => x.Product).Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return t == null ? null : MapToDto(t);
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id, Guid currentUserId, string currentRole)
    {
        var t = await _db.Tickets
            .Include(x => x.Product).Include(x => x.AssignedToUser)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (t == null) return null;

        if (!CanAccessTicket(t, currentUserId, currentRole))
            throw new ForbiddenException("У вас нет доступа к этой заявке");

        return MapToDto(t);
    }

    private static bool CanAccessTicket(Ticket ticket, Guid userId, string role)
    {
        return role switch
        {
            Roles.Admin or Roles.Manager => true,
            Roles.Engineer => ticket.AssignedToUserId == userId,
            Roles.Applicant => ticket.CreatedByUserId == userId,
            _ => false
        };
    }

    public async Task<PagedResult<TicketListItemDto>> GetListAsync(
        TicketFilterDto filter, Guid currentUserId, string currentRole)
    {
        var query = _db.Tickets
            .Include(t => t.Product).Include(t => t.AssignedToUser)
            .Where(t => !t.IsDeleted);

        if (currentRole == Roles.Applicant)
            query = query.Where(t => t.CreatedByUserId == currentUserId);

        if (currentRole == Roles.Engineer)
            query = query.Where(t => t.AssignedToUserId == currentUserId);

        if (filter.Status.HasValue) query = query.Where(t => t.Status == filter.Status.Value);
        if (filter.Priority.HasValue) query = query.Where(t => t.Priority == filter.Priority.Value);
        if (filter.Category.HasValue) query = query.Where(t => t.Category == filter.Category.Value);
        if (filter.ProductId.HasValue) query = query.Where(t => t.ProductId == filter.ProductId.Value);
        if (filter.AssignedToUserId.HasValue) query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId.Value);
        if (filter.Unassigned == true) query = query.Where(t => t.AssignedToUserId == null && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved);
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(t => t.Number.Contains(filter.Search) || t.Title.Contains(filter.Search) || t.Description.Contains(filter.Search));
        if (filter.DateFrom.HasValue) query = query.Where(t => t.CreatedAt >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(t => t.CreatedAt <= filter.DateTo.Value);

        var totalCount = await query.CountAsync();
        query = filter.SortDir == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt);

        var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
            .Select(t => new TicketListItemDto
            {
                Id = t.Id, Number = t.Number, Title = t.Title, Status = t.Status,
                Priority = t.Priority, ProductName = t.Product.Name,
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                CreatedAt = t.CreatedAt
            }).ToListAsync();

        return new PagedResult<TicketListItemDto> { Items = items, TotalCount = totalCount, Page = filter.Page, PageSize = filter.PageSize };
    }

    public async Task<TicketDto> CreateAsync(TicketCreateDto dto, Guid currentUserId)
    {
        var number = await _numberGen.GenerateNextNumberAsync();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(), Number = number, Title = dto.Title, Description = dto.Description,
            ProductId = dto.ProductId, Version = "1", Priority = dto.Priority,
            Status = TicketStatus.New, Category = dto.Category, Impact = dto.Impact, Source = dto.Source,
            AssignedToUserId = dto.AssignedToUserId, CreatedByUserId = currentUserId,
            UpdatedByUserId = currentUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        if (dto.AssignedToUserId.HasValue) ticket.Status = TicketStatus.Assigned;

        _db.Tickets.Add(ticket);
        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticket.Id, ChangedByUserId = currentUserId,
            ChangeType = ChangeType.Creation, NewValue = $"Создана: {ticket.Title}"
        });

        await _audit.LogAsync("Ticket.Create", currentUserId, "Ticket", ticket.Id);
        await _db.SaveChangesAsync();
        return (await GetByIdAsync(ticket.Id))!;
    }

    public async Task<TicketDto> UpdateAsync(Guid id, TicketUpdateDto dto, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(id) ?? throw new NotFoundException("Заявка не найдена");

        var userRole = await GetUserRoleAsync(currentUserId);
        if (!CanAccessTicket(ticket, currentUserId, userRole))
            throw new ForbiddenException("У вас нет прав на редактирование этой заявки");

        if (ticket.Title != dto.Title)
            _db.TicketHistories.Add(new TicketHistory { TicketId = id, ChangedByUserId = currentUserId, ChangeType = ChangeType.Update, FieldName = "Заголовок", OldValue = ticket.Title, NewValue = dto.Title });

        if (ticket.Description != dto.Description)
            _db.TicketHistories.Add(new TicketHistory { TicketId = id, ChangedByUserId = currentUserId, ChangeType = ChangeType.Update, FieldName = "Описание", OldValue = "—", NewValue = "изменено" });

        if (ticket.ProductId != dto.ProductId)
        {
            var oldProduct = await _db.Products.FindAsync(ticket.ProductId);
            var newProduct = await _db.Products.FindAsync(dto.ProductId);
            _db.TicketHistories.Add(new TicketHistory { TicketId = id, ChangedByUserId = currentUserId, ChangeType = ChangeType.Update, FieldName = "Продукт", OldValue = oldProduct?.Name ?? "—", NewValue = newProduct?.Name ?? "—" });
        }

        if (ticket.Priority != dto.Priority)
            _db.TicketHistories.Add(new TicketHistory { TicketId = id, ChangedByUserId = currentUserId, ChangeType = ChangeType.Update, FieldName = "Приоритет", OldValue = ticket.Priority.ToDisplayString(), NewValue = dto.Priority.ToDisplayString() });

        if (ticket.Category != dto.Category)
            _db.TicketHistories.Add(new TicketHistory { TicketId = id, ChangedByUserId = currentUserId, ChangeType = ChangeType.Update, FieldName = "Категория", OldValue = ticket.Category.ToDisplayString(), NewValue = dto.Category.ToDisplayString() });

        if (ticket.Impact != dto.Impact)
            _db.TicketHistories.Add(new TicketHistory { TicketId = id, ChangedByUserId = currentUserId, ChangeType = ChangeType.Update, FieldName = "Влияние", OldValue = ticket.Impact?.ToDisplayString() ?? "—", NewValue = dto.Impact?.ToDisplayString() ?? "—" });

        ticket.Title = dto.Title;
        ticket.Description = dto.Description;
        ticket.ProductId = dto.ProductId;
        ticket.Priority = dto.Priority;
        ticket.Category = dto.Category;
        ticket.Impact = dto.Impact;

        if (int.TryParse(ticket.Version, out var ver))
            ticket.Version = (ver + 1).ToString();
        else
            ticket.Version = "1";

        ticket.UpdatedByUserId = currentUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _audit.LogAsync("Ticket.Update", currentUserId, "Ticket", id);
        await _db.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task ChangeStatusAsync(Guid id, TicketStatus newStatus, string? resolution, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(id) ?? throw new NotFoundException("Заявка не найдена");
        var userRole = await GetUserRoleAsync(currentUserId);

        if (!CanAccessTicket(ticket, currentUserId, userRole))
            throw new ForbiddenException("У вас нет прав на изменение статуса этой заявки");

        if (!IsValidTransition(ticket.Status, newStatus, userRole))
            throw new BusinessRuleException($"Переход {ticket.Status} → {newStatus} недопустим для роли {userRole}");

        var oldStatus = ticket.Status;
        ticket.Status = newStatus; ticket.UpdatedByUserId = currentUserId; ticket.UpdatedAt = DateTime.UtcNow;
        if (newStatus == TicketStatus.Resolved && !string.IsNullOrWhiteSpace(resolution)) ticket.Resolution = resolution;
        if (newStatus == TicketStatus.Closed) ticket.ClosedAt = DateTime.UtcNow;

        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = id, ChangedByUserId = currentUserId, ChangeType = ChangeType.StatusChange,
            FieldName = "Статус", OldValue = oldStatus.ToDisplayString(), NewValue = newStatus.ToDisplayString()
        });

        await _audit.LogAsync("Ticket.StatusChange", currentUserId, "Ticket", id, $"{oldStatus} → {newStatus}");
        await _db.SaveChangesAsync();
    }

    public async Task AssignAsync(Guid id, Guid? assigneeId, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(id) ?? throw new NotFoundException("Заявка не найдена");
        var oldAssigneeId = ticket.AssignedToUserId;
        ticket.AssignedToUserId = assigneeId; ticket.UpdatedByUserId = currentUserId; ticket.UpdatedAt = DateTime.UtcNow;
        if (assigneeId.HasValue && ticket.Status == TicketStatus.New) ticket.Status = TicketStatus.Assigned;

        var oldName = oldAssigneeId.HasValue
            ? await _db.Users.Where(u => u.Id == oldAssigneeId.Value).Select(u => u.FullName + " (" + u.Email + ")").FirstOrDefaultAsync() ?? "—"
            : "—";
        var newName = assigneeId.HasValue
            ? await _db.Users.Where(u => u.Id == assigneeId.Value).Select(u => u.FullName + " (" + u.Email + ")").FirstOrDefaultAsync() ?? "—"
            : "—";

        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = id, ChangedByUserId = currentUserId, ChangeType = ChangeType.Assignment,
            FieldName = "Исполнитель", OldValue = oldName, NewValue = newName
        });
        await _audit.LogAsync("Ticket.Assign", currentUserId, "Ticket", id);
        await _db.SaveChangesAsync();
    }

    public async Task CloseAsync(Guid id, string? resolution, Guid currentUserId)
        => await ChangeStatusAsync(id, TicketStatus.Closed, resolution, currentUserId);

    public async Task ReopenAsync(Guid id, Guid currentUserId)
        => await ChangeStatusAsync(id, TicketStatus.Reopened, null, currentUserId);

    public async Task DeleteAsync(Guid id, Guid currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(id) ?? throw new NotFoundException("Заявка не найдена");
        ticket.IsDeleted = true; ticket.DeletedAt = DateTime.UtcNow; ticket.DeletedByUserId = currentUserId;
        await _audit.LogAsync("Ticket.Delete", currentUserId, "Ticket", id);
        await _db.SaveChangesAsync();
    }

    public async Task<List<TicketHistoryDto>> GetHistoryAsync(Guid ticketId)
    {
        return await _db.TicketHistories.Include(h => h.ChangedByUser)
            .Where(h => h.TicketId == ticketId).OrderByDescending(h => h.ChangedAt)
            .Select(h => new TicketHistoryDto
            {
                Id = h.Id, ChangedByName = h.ChangedByUser.FullName, ChangedAt = h.ChangedAt,
                FieldName = h.FieldName, OldValue = h.OldValue, NewValue = h.NewValue, ChangeType = h.ChangeType
            }).ToListAsync();
    }

    private static bool IsValidTransition(TicketStatus from, TicketStatus to, string role) => (from, to, role) switch
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

    private async Task<string> GetUserRoleAsync(Guid userId)
    {
        var roles = await _db.UserRoles.Where(ur => ur.UserId == userId)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!).ToListAsync();
        return roles.FirstOrDefault() ?? Roles.Applicant;
    }

    private static TicketDto MapToDto(Ticket t) => new()
    {
        Id = t.Id, Number = t.Number, Title = t.Title, Description = t.Description,
        ProductName = t.Product?.Name ?? "", ProductType = t.Product?.ProductType.ToString() ?? "",
        Version = t.Version, Priority = t.Priority, Status = t.Status, Category = t.Category,
        Impact = t.Impact, Source = t.Source, AssignedToUserId = t.AssignedToUserId,
        AssignedToUserName = t.AssignedToUser?.FullName, CreatedByUserName = t.CreatedByUser?.FullName ?? "",
        CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt, ClosedAt = t.ClosedAt,
        Resolution = t.Resolution, TimeSpentMinutes = t.TimeSpentMinutes
    };
}
