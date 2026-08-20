using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Controllers;

[Authorize]
public class FilesController : Controller
{
    private readonly IAttachmentService _attachmentService;
    public FilesController(IAttachmentService attachmentService) => _attachmentService = attachmentService;
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(Guid ticketId, IFormFile file)
    {
        if (file == null || file.Length == 0) { TempData["Error"] = "Файл не выбран"; return RedirectToAction("Details", "Tickets", new { id = ticketId }); }
        using var stream = file.OpenReadStream();
        await _attachmentService.UploadAsync(ticketId, stream, file.FileName, file.ContentType, file.Length, CurrentUserId);
        return RedirectToAction("Details", "Tickets", new { id = ticketId });
    }

    public async Task<IActionResult> Download(Guid id)
    { var (stream, fileName, mimeType) = await _attachmentService.DownloadAsync(id); return File(stream, mimeType, fileName); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid ticketId)
    { await _attachmentService.DeleteAsync(id, CurrentUserId); return RedirectToAction("Details", "Tickets", new { id = ticketId }); }
}
