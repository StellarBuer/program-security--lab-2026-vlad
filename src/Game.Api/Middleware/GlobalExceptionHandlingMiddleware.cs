using Game.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Game.Api.Middleware;

/// <summary>
/// Глобальний Middleware перехоплення та стандартизації винятків.
/// Запобігає витоку системних деталей, SQL-запитів та StackTrace клієнту (STRIDE: Information Disclosure).
/// Транслює доменні винятки AGameEngineException у стандартизовані HTTP-відповіді.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AGameEngineException domainEx)
        {
            _logger.LogWarning(domainEx, "Доменний виняток [{Context}:{ErrorCode}]: {Message}", 
                domainEx.ContextCode, domainEx.ErrorCode, domainEx.Message);

            await HandleDomainExceptionAsync(context, domainEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неперехоплена системна помилка: {Message}", ex.Message);

            await HandleGenericExceptionAsync(context);
        }
    }

    private static Task HandleDomainExceptionAsync(HttpContext context, AGameEngineException ex)
    {
        context.Response.ContentType = "application/json";

        var statusCode = ex switch
        {
            UserNotFoundException => HttpStatusCode.NotFound,
            UserAlreadyExistsException => HttpStatusCode.Conflict,
            InvalidCredentialsException => HttpStatusCode.Unauthorized,
            WeakPasswordException => HttpStatusCode.BadRequest,
            DomainValidationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.BadRequest
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            context = ex.ContextCode,
            errorCode = ex.ErrorCode,
            message = ex.Message,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleGenericExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            context = "CORE",
            errorCode = "INTERNAL_SERVER_ERROR",
            message = "Сталася непередбачена внутрішня помилка сервера. Зверніться до адміністратора.",
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

