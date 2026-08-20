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
    { _userManager = userManager; _signInManager = signInManager; }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(); }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        { ModelState.AddModelError("", "Email и пароль обязательны"); return View(); }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive) { ModelState.AddModelError("", "Неверный email или пароль"); return View(); }

        var result = await _signInManager.PasswordSignInAsync(user, password, false, true);
        if (result.Succeeded) return LocalRedirect(returnUrl ?? "/");
        ModelState.AddModelError("", "Неверный email или пароль");
        return View();
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
    {
        if (password != confirmPassword) { ModelState.AddModelError("", "Пароли не совпадают"); return View(); }
        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, IsActive = true, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded) { await _userManager.AddToRoleAsync(user, Core.Constants.Roles.Applicant); await _signInManager.SignInAsync(user, false); return RedirectToAction("Index", "Dashboard"); }
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        return View();
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await _signInManager.SignOutAsync(); return RedirectToAction("Login"); }

    [HttpGet, Authorize]
    public async Task<IActionResult> Profile() { var user = await _userManager.GetUserAsync(User); return View(user); }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string fullName, string? position, string? phoneNumber)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        user.FullName = fullName; user.Position = position; user.PhoneNumber = phoneNumber; user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        ViewBag.Message = "Профиль обновлён";
        return View(user);
    }

    [HttpGet, Authorize] public IActionResult ChangePassword() => View();

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword) { ModelState.AddModelError("", "Пароли не совпадают"); return View(); }
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded) { await _signInManager.RefreshSignInAsync(user); ViewBag.Message = "Пароль изменён"; return View(); }
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        return View();
    }

    [HttpGet, AllowAnonymous] public IActionResult AccessDenied() => View();
}
