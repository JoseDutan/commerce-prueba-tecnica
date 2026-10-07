using Commerce.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Api.Middleware;

/// <summary>
/// Manejo global de errores: convierte las excepciones en respuestas ProblemDetails (RFC 7807).
/// Los controladores y servicios lanzan excepciones y no repiten bloques try/catch.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning("Petición inválida: {Message}", ex.Message);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Solicitud inválida",
                Detail = ex.Message,
            };
            if (ex.Errors.Count > 0)
            {
                problem.Extensions["errors"] = ex.Errors;
            }

            await WriteAsync(context, problem);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente canceló la petición: no hay a quién responder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Method} {Path}", context.Request.Method, context.Request.Path);

            // No se exponen detalles internos al cliente
            await WriteAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = "Ocurrió un error inesperado. Intente nuevamente.",
            });
        }
    }

    private static Task WriteAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
