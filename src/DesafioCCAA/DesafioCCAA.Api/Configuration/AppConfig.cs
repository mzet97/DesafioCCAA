using DesafioCCAA.Api.Extensions;
using DesafioCCAA.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Configuration;

public static class AppConfig
{
    public static IApplicationBuilder UseAppConfig(this IApplicationBuilder app)
    {
        app.UseResponseCompression();
        app.UseHttpsRedirection();
        app.UseAuthentication();

        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            ExceptionHandler = async context =>
            {
                var exceptionHandler = new CustomExceptionHandler();
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionFeature != null)
                {
                    var exception = exceptionFeature.Error;
                    if (!await exceptionHandler.TryHandleAsync(context, exception, CancellationToken.None))
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Status = StatusCodes.Status500InternalServerError,
                            Title = "An unexpected error occurred.",
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                        });
                    }
                }
            }
        });

        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseCustomSerilogRequestLogging();

        app.UseAuthorization();
        app.UseHealthChecks("/health");
        app.UseStaticFiles();

        return app;
    }
}