using Microsoft.AspNetCore.Builder;
using Serilog;

namespace SistemaLivro.Infrastructure.Middlewares;

public static class SerilogMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomSerilogRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RequestPath", httpContext.Request.Path);
                diagnosticContext.Set("RequestQuery", httpContext.Request.QueryString.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            };

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null || httpContext.Response.StatusCode >= 500)
                {
                    return Serilog.Events.LogEventLevel.Error;
                }

                if (elapsed > 1000)
                {
                    return Serilog.Events.LogEventLevel.Warning;
                }

                return Serilog.Events.LogEventLevel.Information;
            };
        });
    }
}

