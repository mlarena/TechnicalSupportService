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
    { _userManager = userManager; _roleManager = roleManager; _db = db; }

    public async Task<PagedResult<UserDto>> GetListAsync(UserFilterDto filter)
    {
        var query = _db.Users.Include(u => u.Department).AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(u => u.FullName.Contains(filter.Search) || u.Email!.Contains(filter.Search));
        if (filter.IsActive.HasValue) query = query.Where(u => u.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync();
        var users = await query.OrderBy(u => u.FullName)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

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
        return new UserDto { Id = user.Id, FullName = user.FullName, Email = user.Email ?? "", Position = user.Position, DepartmentName = user.Department?.Name, Role = roles.FirstOrDefault() ?? "", IsActive = user.IsActive };
    }

    public async Task<(bool success, List<string> errors)> CreateAsync(UserCreateDto dto)
    {
        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName, Position = dto.Position, DepartmentId = dto.DepartmentId, IsActive = true, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return (false, result.Errors.Select(e => e.Description).ToList());
        await _userManager.AddToRoleAsync(user, dto.Role);
        return (true, new List<string>());
    }

    public async Task<(bool success, List<string> errors)> UpdateAsync(Guid id, UserUpdateDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Пользователь не найден");
        user.FullName = dto.FullName; user.Position = dto.Position; user.DepartmentId = dto.DepartmentId; user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return (false, result.Errors.Select(e => e.Description).ToList());
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(dto.Role)) { await _userManager.RemoveFromRolesAsync(user, currentRoles); await _userManager.AddToRoleAsync(user, dto.Role); }
        return (true, new List<string>());
    }

    public async Task BlockAsync(Guid id, bool block)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Пользователь не найден");
        user.IsActive = !block; user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Пользователь не найден");
        await _userManager.DeleteAsync(user);
    }

    public async Task<List<UserDto>> GetEngineersAsync()
    {
        var engineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer);
        return engineers.Where(u => u.IsActive).Select(u => new UserDto
        { Id = u.Id, FullName = u.FullName, Email = u.Email ?? "", Position = u.Position, Role = Roles.Engineer, IsActive = u.IsActive }).ToList();
    }
}
