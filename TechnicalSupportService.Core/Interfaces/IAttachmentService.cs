using TechnicalSupportService.Core.DTOs;

namespace TechnicalSupportService.Core.Interfaces;

public interface IAttachmentService
{
    Task<List<AttachmentDto>> GetByTicketAsync(Guid ticketId);
    Task<AttachmentDto> UploadAsync(Guid ticketId, Stream fileStream, string fileName, string contentType, long fileSize, Guid currentUserId);
    Task DeleteAsync(Guid id, Guid currentUserId);
    Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(Guid id);
}
