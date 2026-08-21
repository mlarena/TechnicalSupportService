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
        if (currentRole == Roles.Applicant) query = query.Where(t => t.CreatedByUserId == currentUserId);
        if (currentRole == Roles.Engineer) query = query.Where(t => t.AssignedToUserId == currentUserId);

        var ticketsByStatus = await query.GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var ticketsByPriority = await query.Where(t => t.Status != TicketStatus.Closed)
            .GroupBy(t => t.Priority).Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count);

        var recentTickets = await query.OrderByDescending(t => t.CreatedAt).Take(10)
            .Select(t => new TicketListItemDto
            {
                Id = t.Id, Number = t.Number, Title = t.Title, Status = t.Status, Priority = t.Priority,
                ProductName = t.Product.Name, AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                CreatedAt = t.CreatedAt
            }).ToListAsync();

        var criticalCount = await query.CountAsync(t => t.Priority == Priority.Critical && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved);
        var unassignedCount = await query.CountAsync(t => t.AssignedToUserId == null && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved);
        var totalOpen = await query.CountAsync(t => t.Status != TicketStatus.Closed);
        var inProgressCount = await query.CountAsync(t => t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Reopened);

        return new DashboardDto
        {
            TicketsByStatus = ticketsByStatus, TicketsByPriority = ticketsByPriority,
            RecentTickets = recentTickets, CriticalCount = criticalCount,
            UnassignedCount = unassignedCount, TotalOpen = totalOpen,
            InProgressCount = inProgressCount
        };
    }
}
