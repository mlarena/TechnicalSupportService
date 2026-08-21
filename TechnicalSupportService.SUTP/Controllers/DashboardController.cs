using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Helpers;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ITicketService _ticketService;

    public DashboardController(IDashboardService dashboardService, ITicketService ticketService)
    {
        _dashboardService = dashboardService;
        _ticketService = ticketService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Applicant";
        var dashboard = await _dashboardService.GetDashboardAsync(userId, role);
        return View(dashboard);
    }

    [HttpGet]
    public async Task<IActionResult> GetFilteredTickets(
        TicketStatus? status, Priority? priority, bool? unassigned)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Applicant";

        var filter = new TicketFilterDto
        {
            Status = status,
            Priority = priority,
            Unassigned = unassigned,
            PageSize = 100
        };

        var result = await _ticketService.GetListAsync(filter, userId, role);

        var tickets = result.Items.Select(t => new
        {
            id = t.Id,
            number = t.Number,
            title = t.Title,
            status = t.Status.ToString(),
            statusDisplay = t.Status.ToDisplayString(),
            priority = t.Priority.ToString(),
            priorityDisplay = t.Priority.ToDisplayString(),
            productName = t.ProductName,
            assignedToUserName = t.AssignedToUserName,
            createdAt = t.CreatedAt.ToString("dd.MM.yyyy HH:mm")
        });

        return Json(tickets);
    }
}
