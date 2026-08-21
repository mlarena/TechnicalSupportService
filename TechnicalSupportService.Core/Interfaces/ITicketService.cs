using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.Interfaces;

public interface ITicketService
{
    Task<TicketDto?> GetByIdAsync(Guid id);
    Task<TicketDto?> GetByIdAsync(Guid id, Guid currentUserId, string currentRole);
    Task<PagedResult<TicketListItemDto>> GetListAsync(TicketFilterDto filter, Guid currentUserId, string currentRole);
    Task<TicketDto> CreateAsync(TicketCreateDto dto, Guid currentUserId);
    Task<TicketDto> UpdateAsync(Guid id, TicketUpdateDto dto, Guid currentUserId);
    Task ChangeStatusAsync(Guid id, TicketStatus newStatus, string? resolution, Guid currentUserId);
    Task AssignAsync(Guid id, Guid? assigneeId, Guid currentUserId);
    Task CloseAsync(Guid id, string? resolution, Guid currentUserId);
    Task ReopenAsync(Guid id, Guid currentUserId);
    Task DeleteAsync(Guid id, Guid currentUserId);
    Task<List<TicketHistoryDto>> GetHistoryAsync(Guid ticketId);
}
