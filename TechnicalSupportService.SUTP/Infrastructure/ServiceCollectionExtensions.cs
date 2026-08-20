using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.SUTP.Services;

namespace TechnicalSupportService.SUTP.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<INumberGeneratorService, NumberGeneratorService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
