using TechnicalSupportService.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace TechnicalSupportService.Core.Interfaces;

public interface IAttachmentService
{
    Task<List<AttachmentDto>> GetByTicketAsync(Guid ticketId);
    Task<AttachmentDto> UploadAsync(Guid ticketId, IFormFile file, Guid currentUserId);
    Task DeleteAsync(Guid id, Guid currentUserId);
    Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(Guid id);
}
