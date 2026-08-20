using TechnicalSupportService.Core.Exceptions;

namespace TechnicalSupportService.SUTP.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (NotFoundException ex) { _logger.LogWarning(ex, "Not found"); context.Response.StatusCode = 404; await WriteJson(context, ex.Message); }
        catch (ForbiddenException ex) { _logger.LogWarning(ex, "Forbidden"); context.Response.StatusCode = 403; await WriteJson(context, ex.Message); }
        catch (BusinessRuleException ex) { _logger.LogWarning(ex, "Business rule"); context.Response.StatusCode = 400; await WriteJson(context, ex.Message); }
        catch (Exception ex) { _logger.LogError(ex, "Unhandled"); context.Response.StatusCode = 500; await WriteJson(context, "Внутренняя ошибка сервера"); }
    }

    private static async Task WriteJson(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { error = message }));
    }
}
