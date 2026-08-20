# Шаг 3. Контроллеры — точный C# код

> Все файлы в проекте `TechnicalSupportService.SUTP`.

---

## 3.1. Controllers/AccountController.cs

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email и пароль обязательны");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError("", "Неверный email или пароль");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, false, true);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? "/");
        }

        ModelState.AddModelError("", "Неверный email или пароль");
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ModelState.AddModelError("", "Пароли не совпадают");
            return View();
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Core.Constants.Roles.Applicant);
            await _signInManager.SignInAsync(user, false);
            return RedirectToAction("Index", "Dashboard");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        return View(user);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string fullName, string? position, string? phoneNumber)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FullName = fullName;
        user.Position = position;
        user.PhoneNumber = phoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        ViewBag.Message = "Профиль обновлён";
        return View(user);
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View();

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "Новые пароли не совпадают");
            return View();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            ViewBag.Message = "Пароль изменён";
            return View();
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
```

---

## 3.2. Controllers/DashboardController.cs

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Applicant";

        var dashboard = await _dashboardService.GetDashboardAsync(userId, role);
        return View(dashboard);
    }
}
```

---

## 3.3. Controllers/TicketsController.cs

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ITicketService _ticketService;
    private readonly ICommentService _commentService;
    private readonly IAttachmentService _attachmentService;
    private readonly IProductService _productService;
    private readonly IUserService _userService;

    public TicketsController(
        ITicketService ticketService, ICommentService commentService,
        IAttachmentService attachmentService, IProductService productService,
        IUserService userService)
    {
        _ticketService = ticketService;
        _commentService = commentService;
        _attachmentService = attachmentService;
        _productService = productService;
        _userService = userService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole => User.FindFirstValue(ClaimTypes.Role) ?? Roles.Applicant;

    public async Task<IActionResult> Index(TicketFilterDto filter)
    {
        var result = await _ticketService.GetListAsync(filter, CurrentUserId, CurrentRole);
        ViewBag.Filter = filter;
        ViewBag.Products = await _productService.GetAllAsync();
        ViewBag.Engineers = await _userService.GetEngineersAsync();
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await _productService.GetAllAsync();
        ViewBag.Engineers = await _userService.GetEngineersAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _productService.GetAllAsync();
            ViewBag.Engineers = await _userService.GetEngineersAsync();
            return View(dto);
        }

        var ticket = await _ticketService.CreateAsync(dto, CurrentUserId);
        return RedirectToAction("Details", new { id = ticket.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var ticket = await _ticketService.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        var comments = await _commentService.GetByTicketAsync(id, CurrentRole);
        var attachments = await _attachmentService.GetByTicketAsync(id);
        var history = await _ticketService.GetHistoryAsync(id);

        ViewBag.Comments = comments;
        ViewBag.Attachments = attachments;
        ViewBag.History = history;
        ViewBag.CurrentRole = CurrentRole;
        ViewBag.CurrentUserId = CurrentUserId;

        return View(ticket);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var ticket = await _ticketService.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        ViewBag.Products = await _productService.GetAllAsync();
        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TicketUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _productService.GetAllAsync();
            return View(await _ticketService.GetByIdAsync(id));
        }

        await _ticketService.UpdateAsync(id, dto, CurrentUserId);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, TicketStatus newStatus, string? resolution)
    {
        await _ticketService.ChangeStatusAsync(id, newStatus, resolution, CurrentUserId);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Assign(Guid id, Guid? assigneeId)
    {
        await _ticketService.AssignAsync(id, assigneeId, CurrentUserId);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(Guid id, CommentCreateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Content))
            await _commentService.AddAsync(id, dto, CurrentUserId);

        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id, string? resolution)
    {
        await _ticketService.CloseAsync(id, resolution, CurrentUserId);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(Guid id)
    {
        await _ticketService.ReopenAsync(id, CurrentUserId);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _ticketService.DeleteAsync(id, CurrentUserId);
        return RedirectToAction("Index");
    }
}
```

---

## 3.4. Controllers/AdminController.cs

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IProductService _productService;
    private readonly IDepartmentService _departmentService;
    private readonly IAuditService _auditService;

    public AdminController(IUserService userService, IProductService productService,
        IDepartmentService departmentService, IAuditService auditService)
    {
        _userService = userService;
        _productService = productService;
        _departmentService = departmentService;
        _auditService = auditService;
    }

    // === Пользователи ===
    public async Task<IActionResult> Users(UserFilterDto filter)
    {
        var result = await _userService.GetListAsync(filter);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        ViewBag.Roles = Roles.All;
        ViewBag.Departments = await _departmentService.GetAllAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(UserCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = Roles.All;
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(dto);
        }

        var (success, errors) = await _userService.CreateAsync(dto);
        if (!success)
        {
            foreach (var e in errors) ModelState.AddModelError("", e);
            ViewBag.Roles = Roles.All;
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(dto);
        }

        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.Roles = Roles.All;
        ViewBag.Departments = await _departmentService.GetAllAsync();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(Guid id, UserUpdateDto dto)
    {
        var (success, errors) = await _userService.UpdateAsync(id, dto);
        if (!success)
        {
            foreach (var e in errors) ModelState.AddModelError("", e);
        }
        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(Guid id, bool block)
    {
        await _userService.BlockAsync(id, block);
        return RedirectToAction("Users");
    }

    // === Продукты ===
    public async Task<IActionResult> Products()
    {
        var products = await _productService.GetAllAsync(includeInactive: true);
        return View(products);
    }

    [HttpGet]
    public IActionResult CreateProduct() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _productService.CreateAsync(dto);
        return RedirectToAction("Products");
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product == null ? NotFound() : View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Guid id, ProductCreateDto dto)
    {
        await _productService.UpdateAsync(id, dto);
        return RedirectToAction("Products");
    }

    // === Отделы ===
    public async Task<IActionResult> Departments()
    {
        var depts = await _departmentService.GetAllAsync(includeInactive: true);
        return View(depts);
    }

    // === Аудит ===
    public IActionResult AuditLog()
    {
        return View();
    }
}
```

---

## 3.5. Controllers/FilesController.cs

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Controllers;

[Authorize]
public class FilesController : Controller
{
    private readonly IAttachmentService _attachmentService;

    public FilesController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(Guid ticketId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Файл не выбран";
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }

        await _attachmentService.UploadAsync(ticketId, file, CurrentUserId);
        return RedirectToAction("Details", "Tickets", new { id = ticketId });
    }

    public async Task<IActionResult> Download(Guid id)
    {
        var (stream, fileName, mimeType) = await _attachmentService.DownloadAsync(id);
        return File(stream, mimeType, fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid ticketId)
    {
        await _attachmentService.DeleteAsync(id, CurrentUserId);
        return RedirectToAction("Details", "Tickets", new { id = ticketId });
    }
}
```

---

## 3.6. Middleware/ExceptionHandlingMiddleware.cs

```csharp
using System.Net;
using System.Text.Json;
using TechnicalSupportService.Core.Exceptions;

namespace TechnicalSupportService.SUTP.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteJson(context, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Forbidden");
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await WriteJson(context, ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteJson(context, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteJson(context, "Внутренняя ошибка сервера");
        }
    }

    private static async Task WriteJson(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
```
