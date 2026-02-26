using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace BackendPOS.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        } 
        
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized");
            await WriteError(context, HttpStatusCode.NotFound, ex.Message);
        } 
        
        catch(KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Not Found");
            await WriteError(context, HttpStatusCode.NotFound, ex.Message);
        } 
        
        catch(ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad Request");
            await WriteError(context, HttpStatusCode.BadRequest, ex.Message);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            await WriteError(context, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private static async Task WriteError(HttpContext context, HttpStatusCode code, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var payload = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(payload);
    }
}