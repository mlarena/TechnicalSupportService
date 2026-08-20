namespace TechnicalSupportService.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string originalFileName, Guid ticketId);
    Task<Stream> ReadAsync(string filePath);
    Task DeleteAsync(string filePath);
}

public interface INumberGeneratorService
{
    Task<string> GenerateNextNumberAsync();
}

public interface IAuditService
{
    Task LogAsync(string action, Guid userId, string? entityName = null,
        Guid? entityId = null, string? details = null,
        string? ipAddress = null, string? userAgent = null);
}
