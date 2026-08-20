using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;

namespace TechnicalSupportService.SUTP.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private static readonly HashSet<string> AllowedExtensions = new(
        new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".png", ".zip" },
        StringComparer.OrdinalIgnoreCase);

    public LocalFileStorageService(IConfiguration config)
    {
        _basePath = config["FileStorage:LocalPath"]
            ?? throw new InvalidOperationException("FileStorage:LocalPath not configured");
    }

    public async Task<string> SaveAsync(Stream fileStream, string originalFileName, Guid ticketId)
    {
        var ext = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(ext))
            throw new BusinessRuleException($"Формат {ext} не допустим");

        var storedName = $"{Guid.NewGuid()}{ext}";
        var relativePath = Path.Combine(ticketId.ToString(), storedName);
        var fullPath = Path.Combine(_basePath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output);
        return relativePath;
    }

    public Task<Stream> ReadAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath)) throw new NotFoundException("Файл не найден на диске");
        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
