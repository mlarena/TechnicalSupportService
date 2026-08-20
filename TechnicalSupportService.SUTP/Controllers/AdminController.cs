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

    public AdminController(IUserService userService, IProductService productService, IDepartmentService departmentService)
    { _userService = userService; _productService = productService; _departmentService = departmentService; }

    public async Task<IActionResult> Users(UserFilterDto filter)
    { var result = await _userService.GetListAsync(filter); ViewBag.Filter = filter; return View(result); }

    [HttpGet] public async Task<IActionResult> CreateUser()
    { ViewBag.Roles = Roles.All; ViewBag.Departments = await _departmentService.GetAllAsync(); return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(UserCreateDto dto)
    {
        if (!ModelState.IsValid) { ViewBag.Roles = Roles.All; ViewBag.Departments = await _departmentService.GetAllAsync(); return View(dto); }
        var (success, errors) = await _userService.CreateAsync(dto);
        if (!success) { foreach (var e in errors) ModelState.AddModelError("", e); ViewBag.Roles = Roles.All; return View(dto); }
        return RedirectToAction("Users");
    }

    [HttpGet] public async Task<IActionResult> EditUser(Guid id)
    { var user = await _userService.GetByIdAsync(id); if (user == null) return NotFound(); ViewBag.Roles = Roles.All; ViewBag.Departments = await _departmentService.GetAllAsync(); return View(user); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(Guid id, UserUpdateDto dto)
    { await _userService.UpdateAsync(id, dto); return RedirectToAction("Users"); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(Guid id, bool block)
    { await _userService.BlockAsync(id, block); return RedirectToAction("Users"); }

    public async Task<IActionResult> Products() => View(await _productService.GetAllAsync(includeInactive: true));

    [HttpGet] public IActionResult CreateProduct() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
    { if (!ModelState.IsValid) return View(dto); await _productService.CreateAsync(dto); return RedirectToAction("Products"); }

    [HttpGet] public async Task<IActionResult> EditProduct(Guid id)
    { var p = await _productService.GetByIdAsync(id); return p == null ? NotFound() : View(p); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Guid id, ProductCreateDto dto)
    { await _productService.UpdateAsync(id, dto); return RedirectToAction("Products"); }

    public async Task<IActionResult> Departments() => View(await _departmentService.GetAllAsync(includeInactive: true));

    public IActionResult AuditLog() => View();
}
