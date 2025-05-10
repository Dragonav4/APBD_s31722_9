using System.Net;
using System.Text.Json;

namespace APBD_s31722_9_APi_2.Exceptions;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)GetStatusCode(ex);
            var payload = JsonSerializer.Serialize(new {error = ex.Message, stacktrace = ex.StackTrace});
            await context.Response.WriteAsync(payload);
        }
    }

    private static HttpStatusCode GetStatusCode(Exception ex)
    {
        if (ex is BadRequestException badRequestException)
            return badRequestException.StatusCode;
        if (ex is InternalServerErrorException serverException)
            return serverException.StatusCode;
        return HttpStatusCode.InternalServerError;
    }
}