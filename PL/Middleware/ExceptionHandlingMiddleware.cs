using System.Diagnostics.CodeAnalysis;

namespace PL.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        _next = next;
        _logger = logger;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Top-level exception handler intentionally catches all unhandled exceptions.")]
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entity not found: {Message}", ex.Message);
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (DuplicateEntityException ex)
        {
            _logger.LogWarning(ex, "Duplicate entity: {Message}", ex.Message);
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (AuthorizationException ex)
        {
            _logger.LogWarning(ex, "Forbidden: {Message}", ex.Message);
            await WriteErrorAsync(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new ErrorResponse(message));
    }
}
