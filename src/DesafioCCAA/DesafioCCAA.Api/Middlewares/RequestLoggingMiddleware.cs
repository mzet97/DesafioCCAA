using Serilog;
using System.Security.Claims;
using System.Text.Json;

namespace DesafioCCAA.Api.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        if (context.Request.Body == null)
        {
            await _next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/swagger") || context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        string body = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        bool isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        var userClaims = isAuthenticated
            ? context.User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            : null;

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = context.User.Identity?.Name;

        var headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

        if (isAuthenticated)
        {
            Log.Information("Usuário autenticado: {UserName} ({UserId})", userName, userId);
        }
        else
        {
            Log.Information("Usuário não autenticado");
        }

        var entityLog = new EntityLog
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            Body = body,
            Headers = JsonSerializer.Serialize(headers),
            ClientIp = clientIp,
            UserAgent = userAgent,
            UserName = userName,
            UserClaims = JsonSerializer.Serialize(userClaims),
            IsAuthenticated = isAuthenticated
        };

        Log.Information("HTTP Request Information: {entityLog}",
            entityLog);

        await _next(context);
    }

}

public record EntityLog
{
    public string Method { get; init; }
    public string Path { get; init; }
    public string QueryString { get; init; }
    public string Body { get; init; }
    public string Headers { get; init; }
    public string ClientIp { get; init; }
    public string UserAgent { get; init; }
    public string UserName { get; init; }
    public string UserClaims { get; init; }
    public bool IsAuthenticated { get; init; }

    public override string? ToString()
    {
        return @$"IsAuthenticated: {IsAuthenticated}, Method: {Method}, Path: {Path}, QueryString: {QueryString}, Body: {Body}, Headers: {Headers}, ClientIp: {ClientIp}, UserAgent: {UserAgent}, UserName: {UserName}, UserClaims: {UserClaims}";
    }
}

