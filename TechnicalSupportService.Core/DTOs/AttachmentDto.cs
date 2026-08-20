namespace TechnicalSupportService.Core.DTOs;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = "";
    public string UploadedByName { get; set; } = "";
    public DateTime UploadedAt { get; set; }
}
