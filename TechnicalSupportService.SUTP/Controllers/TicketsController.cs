using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;

using TechnicalSupportService.Core.Helpers;

namespace TechnicalSupportService.SUTP.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ITicketService _ticketService;
    private readonly ICommentService _commentService;
    private readonly IAttachmentService _attachmentService;
    private readonly IProductService _productService;
    private readonly IUserService _userService;

    public TicketsController(ITicketService ticketService, ICommentService commentService,
        IAttachmentService attachmentService, IProductService productService, IUserService userService)
    { _ticketService = ticketService; _commentService = commentService; _attachmentService = attachmentService; _productService = productService; _userService = userService; }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole => User.FindFirstValue(ClaimTypes.Role) ?? Roles.Applicant;

    public async Task<IActionResult> Index(TicketFilterDto filter)
    {
        var result = await _ticketService.GetListAsync(filter, CurrentUserId, CurrentRole);
        ViewBag.Filter = filter;
        ViewBag.Products = new SelectList(await _productService.GetAllAsync(), "Id", "Name", filter.ProductId);
        ViewBag.Engineers = new SelectList(await _userService.GetEngineersAsync(), "Id", "FullName", filter.AssignedToUserId);
        return View(result);
    }

    [HttpGet] public async Task<IActionResult> Create()
    {
        ViewBag.Products = new SelectList(await _productService.GetAllAsync(), "Id", "Name");
        ViewBag.Engineers = new SelectList(await _userService.GetEngineersAsync(), "Id", "FullName");
        ViewBag.Priorities = new SelectList(Enum.GetValues<Priority>().Select(v => new { Value = v, Text = v.ToDisplayString() }), "Value", "Text");
        ViewBag.Categories = new SelectList(Enum.GetValues<Category>().Select(v => new { Value = v, Text = v.ToDisplayString() }), "Value", "Text");
        ViewBag.Sources = new SelectList(Enum.GetValues<Source>().Select(v => new { Value = v, Text = v.ToDisplayString() }), "Value", "Text");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateDto dto)
    {
        if (!ModelState.IsValid) { ViewBag.Products = new SelectList(await _productService.GetAllAsync(), "Id", "Name"); return View(dto); }
        var ticket = await _ticketService.CreateAsync(dto, CurrentUserId);
        return RedirectToAction("Details", new { id = ticket.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        TicketDto? ticket;
        try { ticket = await _ticketService.GetByIdAsync(id, CurrentUserId, CurrentRole); }
        catch (ForbiddenException) { return RedirectToAction("AccessDenied", "Account"); }
        if (ticket == null) return NotFound();
        ViewBag.Comments = await _commentService.GetByTicketAsync(id, CurrentRole);
        ViewBag.Attachments = await _attachmentService.GetByTicketAsync(id);
        ViewBag.History = await _ticketService.GetHistoryAsync(id);
        ViewBag.CurrentRole = CurrentRole;
        ViewBag.CurrentUserId = CurrentUserId;

        // Engineers for assign dropdown
        ViewBag.Engineers = new SelectList(await _userService.GetEngineersAsync(), "Id", "FullName", ticket.AssignedToUserId);

        // Valid status transitions
        ViewBag.ValidStatuses = GetValidTransitions(ticket.Status, CurrentRole);

        return View(ticket);
    }

    private static List<TicketStatus> GetValidTransitions(TicketStatus current, string role) => (current, role) switch
    {
        (TicketStatus.New, Roles.Admin or Roles.Manager) => [TicketStatus.Assigned, TicketStatus.Closed],
        (TicketStatus.Assigned, Roles.Engineer) => [TicketStatus.InProgress],
        (TicketStatus.InProgress, Roles.Engineer) => [TicketStatus.Resolved],
        (TicketStatus.Resolved, _) => [TicketStatus.Closed, TicketStatus.Reopened],
        (TicketStatus.Closed, Roles.Applicant or Roles.Manager or Roles.Admin) => [TicketStatus.Reopened],
        (_, Roles.Admin or Roles.Manager) => [TicketStatus.Closed],
        _ => new List<TicketStatus>()
    };

    [HttpGet] public async Task<IActionResult> Edit(Guid id)
    {
        TicketDto? ticket;
        try { ticket = await _ticketService.GetByIdAsync(id, CurrentUserId, CurrentRole); }
        catch (ForbiddenException) { return RedirectToAction("AccessDenied", "Account"); }
        if (ticket == null) return NotFound();
        ViewBag.Products = new SelectList(await _productService.GetAllAsync(), "Id", "Name");
        return View(ticket);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TicketUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products = new SelectList(await _productService.GetAllAsync(), "Id", "Name");
            TicketDto? ticket;
            try { ticket = await _ticketService.GetByIdAsync(id, CurrentUserId, CurrentRole); }
            catch (ForbiddenException) { return RedirectToAction("AccessDenied", "Account"); }
            return View(ticket);
        }
        await _ticketService.UpdateAsync(id, dto, CurrentUserId);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, TicketStatus newStatus, string? resolution)
    {
        try { await _ticketService.ChangeStatusAsync(id, newStatus, resolution, CurrentUserId); }
        catch (ForbiddenException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction("Details", new { id }); }
        catch (BusinessRuleException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction("Details", new { id }); }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Assign(Guid id, Guid? assigneeId)
    { await _ticketService.AssignAsync(id, assigneeId, CurrentUserId); return RedirectToAction("Details", new { id }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(Guid id, CommentCreateDto dto)
    { if (!string.IsNullOrWhiteSpace(dto.Content)) await _commentService.AddAsync(id, dto, CurrentUserId); return RedirectToAction("Details", new { id, tab = "comments" }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id, string? resolution)
    {
        try { await _ticketService.CloseAsync(id, resolution, CurrentUserId); }
        catch (ForbiddenException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction("Details", new { id }); }
        catch (BusinessRuleException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction("Details", new { id }); }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(Guid id)
    {
        try { await _ticketService.ReopenAsync(id, CurrentUserId); }
        catch (ForbiddenException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction("Details", new { id }); }
        catch (BusinessRuleException ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction("Details", new { id }); }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    { await _ticketService.DeleteAsync(id, CurrentUserId); return RedirectToAction("Index"); }
}
