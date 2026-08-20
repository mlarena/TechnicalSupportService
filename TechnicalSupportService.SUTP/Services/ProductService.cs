using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;
    public ProductService(ApplicationDbContext db) => _db = db;

    public async Task<List<ProductDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.Products.AsQueryable();
        if (!includeInactive) query = query.Where(p => p.IsActive);
        return await query.OrderBy(p => p.Name).Select(p => new ProductDto
        {
            Id = p.Id, Name = p.Name, ProductType = p.ProductType,
            CurrentVersion = p.CurrentVersion, Description = p.Description, IsActive = p.IsActive
        }).ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id) => await _db.Products.Where(p => p.Id == id)
        .Select(p => new ProductDto { Id = p.Id, Name = p.Name, ProductType = p.ProductType, CurrentVersion = p.CurrentVersion, Description = p.Description, IsActive = p.IsActive })
        .FirstOrDefaultAsync();

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
    {
        var product = new Product { Name = dto.Name, ProductType = dto.ProductType, CurrentVersion = dto.CurrentVersion, Description = dto.Description };
        _db.Products.Add(product); await _db.SaveChangesAsync();
        return new ProductDto { Id = product.Id, Name = product.Name, ProductType = product.ProductType, CurrentVersion = product.CurrentVersion, Description = product.Description, IsActive = true };
    }

    public async Task<ProductDto> UpdateAsync(Guid id, ProductCreateDto dto)
    {
        var product = await _db.Products.FindAsync(id) ?? throw new NotFoundException("Продукт не найден");
        product.Name = dto.Name; product.ProductType = dto.ProductType; product.CurrentVersion = dto.CurrentVersion;
        product.Description = dto.Description; product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new ProductDto { Id = product.Id, Name = product.Name, ProductType = product.ProductType, CurrentVersion = product.CurrentVersion, Description = product.Description, IsActive = product.IsActive };
    }
}
