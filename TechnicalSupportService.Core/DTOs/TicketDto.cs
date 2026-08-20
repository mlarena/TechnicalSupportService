using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.DTOs;

public class TicketDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string ProductType { get; set; } = "";
    public string? Version { get; set; }
    public Priority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public Category Category { get; set; }
    public Impact? Impact { get; set; }
    public Source Source { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string CreatedByUserName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Resolution { get; set; }
    public int? TimeSpentMinutes { get; set; }
}

public class TicketListItemDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public TicketStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string ProductName { get; set; } = "";
    public string? AssignedToUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketCreateDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Заголовок обязателен")]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Title { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Описание обязательно")]
    public string Description { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Выберите продукт")]
    public Guid ProductId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public Priority Priority { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public Category Category { get; set; }

    public Impact? Impact { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public Source Source { get; set; }

    public Guid? AssignedToUserId { get; set; }
}

public class TicketUpdateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Title { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required]
    public string Description { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required]
    public Guid ProductId { get; set; }

    public Priority Priority { get; set; }
    public Category Category { get; set; }
    public Impact? Impact { get; set; }
}

public class TicketFilterDto
{
    public TicketStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public Category? Category { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Search { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortDir { get; set; } = "desc";
}

public class TicketHistoryDto
{
    public Guid Id { get; set; }
    public string ChangedByName { get; set; } = "";
    public DateTime ChangedAt { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public ChangeType ChangeType { get; set; }
}
