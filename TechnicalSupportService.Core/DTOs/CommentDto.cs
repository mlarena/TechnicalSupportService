namespace TechnicalSupportService.Core.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = "";
    public string Content { get; set; } = "";
    public bool IsInternal { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CommentCreateDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Текст обязателен")]
    public string Content { get; set; } = "";

    public bool IsInternal { get; set; } = false;
}
