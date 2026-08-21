using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public ProductType ProductType { get; set; }
    public string? CurrentVersion { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class ProductCreateDto
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = "";
    public ProductType ProductType { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? CurrentVersion { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Description { get; set; }
}

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class DepartmentCreateDto
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = "";
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Description { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Position { get; set; }
    public string? DepartmentName { get; set; }
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
}

public class UserFilterDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class UserCreateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string FullName { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(8)]
    public string Password { get; set; } = "";
    public string? Position { get; set; }
    public Guid? DepartmentId { get; set; }
    [System.ComponentModel.DataAnnotations.Required]
    public string Role { get; set; } = "";
}

public class UserUpdateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string FullName { get; set; } = "";
    public string? Position { get; set; }
    public Guid? DepartmentId { get; set; }
    public string Role { get; set; } = "";
}

public class DashboardDto
{
    public Dictionary<string, int> TicketsByStatus { get; set; } = new();
    public Dictionary<string, int> TicketsByPriority { get; set; } = new();
    public List<TicketListItemDto> RecentTickets { get; set; } = new();
    public int CriticalCount { get; set; }
    public int UnassignedCount { get; set; }
    public int TotalOpen { get; set; }
    public int InProgressCount { get; set; }
}
