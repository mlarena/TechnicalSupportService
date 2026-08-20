using TechnicalSupportService.Core.DTOs;

namespace TechnicalSupportService.Core.Interfaces;

public interface ICommentService
{
    Task<List<CommentDto>> GetByTicketAsync(Guid ticketId, string currentRole);
    Task<CommentDto> AddAsync(Guid ticketId, CommentCreateDto dto, Guid currentUserId);
    Task<CommentDto> EditAsync(Guid commentId, string newContent, Guid currentUserId);
    Task DeleteAsync(Guid commentId, Guid currentUserId);
}
