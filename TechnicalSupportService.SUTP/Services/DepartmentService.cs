using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Exceptions;
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
        { Id = d.Id, Name = d.Name, Description = d.Description, IsActive = d.IsActive }).ToListAsync();
    }

    public async Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto)
    {
        var dept = new Department { Name = dto.Name, Description = dto.Description };
        _db.Departments.Add(dept); await _db.SaveChangesAsync();
        return new DepartmentDto { Id = dept.Id, Name = dept.Name, Description = dept.Description, IsActive = true };
    }

    public async Task<DepartmentDto> UpdateAsync(Guid id, DepartmentCreateDto dto)
    {
        var dept = await _db.Departments.FindAsync(id) ?? throw new NotFoundException("Отдел не найден");
        dept.Name = dto.Name; dept.Description = dto.Description;
        await _db.SaveChangesAsync();
        return new DepartmentDto { Id = dept.Id, Name = dept.Name, Description = dept.Description, IsActive = dept.IsActive };
    }
}
