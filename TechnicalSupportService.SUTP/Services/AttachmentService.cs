using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class AttachmentService : IAttachmentService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly IAuditService _audit;

    public AttachmentService(ApplicationDbContext db, IFileStorageService fileStorage, IAuditService audit)
    { _db = db; _fileStorage = fileStorage; _audit = audit; }

    public async Task<List<AttachmentDto>> GetByTicketAsync(Guid ticketId)
    {
        return await _db.Attachments.Include(a => a.UploadedByUser)
            .Where(a => a.TicketId == ticketId && !a.IsDeleted)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentDto
            {
                Id = a.Id, FileName = a.FileName, FileSizeBytes = a.FileSizeBytes,
                MimeType = a.MimeType, UploadedByName = a.UploadedByUser.FullName, UploadedAt = a.UploadedAt
            }).ToListAsync();
    }

    public async Task<AttachmentDto> UploadAsync(Guid ticketId, Microsoft.AspNetCore.Http.IFormFile file, Guid currentUserId)
    {
        using var stream = file.OpenReadStream();
        var filePath = await _fileStorage.SaveAsync(stream, file.FileName, ticketId);

        var attachment = new Attachment
        {
            TicketId = ticketId, FileName = file.FileName, StoredFileName = Path.GetFileName(filePath),
            FilePath = filePath, FileSizeBytes = file.Length, MimeType = file.ContentType,
            UploadedByUserId = currentUserId, UploadedAt = DateTime.UtcNow
        };
        _db.Attachments.Add(attachment);
        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticketId, ChangedByUserId = currentUserId, ChangeType = ChangeType.FileAttach,
            AttachmentId = attachment.Id, NewValue = file.FileName
        });
        await _audit.LogAsync("File.Upload", currentUserId, "Attachment", attachment.Id);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(currentUserId);
        return new AttachmentDto
        {
            Id = attachment.Id, FileName = attachment.FileName, FileSizeBytes = attachment.FileSizeBytes,
            MimeType = attachment.MimeType, UploadedByName = user?.FullName ?? "", UploadedAt = attachment.UploadedAt
        };
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId)
    {
        var attachment = await _db.Attachments.FindAsync(id) ?? throw new NotFoundException("Файл не найден");
        attachment.IsDeleted = true; attachment.DeletedAt = DateTime.UtcNow;
        await _fileStorage.DeleteAsync(attachment.FilePath);
        await _audit.LogAsync("File.Delete", currentUserId, "Attachment", id);
        await _db.SaveChangesAsync();
    }

    public async Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(Guid id)
    {
        var attachment = await _db.Attachments.FindAsync(id) ?? throw new NotFoundException("Файл не найден");
        if (attachment.IsDeleted) throw new NotFoundException("Файл был удалён");
        var stream = await _fileStorage.ReadAsync(attachment.FilePath);
        return (stream, attachment.FileName, attachment.MimeType);
    }
}
