using TechnicalSupportService.Core.DTOs;

namespace TechnicalSupportService.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid currentUserId, string currentRole);
}

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(bool includeInactive = false);
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(ProductCreateDto dto);
    Task<ProductDto> UpdateAsync(Guid id, ProductCreateDto dto);
}

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync(bool includeInactive = false);
    Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto);
    Task<DepartmentDto> UpdateAsync(Guid id, DepartmentCreateDto dto);
}

public interface IUserService
{
    Task<PagedResult<UserDto>> GetListAsync(UserFilterDto filter);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<(bool success, List<string> errors)> CreateAsync(UserCreateDto dto);
    Task<(bool success, List<string> errors)> UpdateAsync(Guid id, UserUpdateDto dto);
    Task BlockAsync(Guid id, bool block);
    Task DeleteAsync(Guid id);
    Task<List<UserDto>> GetEngineersAsync();
}
