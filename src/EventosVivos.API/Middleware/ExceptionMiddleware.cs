using System.Net;
using System.Text.Json;
using EventosVivos.Domain.Exceptions;

namespace EventosVivos.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso no encontrado");
            await WriteResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Violacion de regla de negocio");
            await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno no controlado");
            await WriteResponse(context, HttpStatusCode.InternalServerError, "Error interno del servidor.");
        }
    }

    private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var response = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(response);
    }
}

