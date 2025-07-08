using SistemaLivro.Infrastructure.Configuration;
using SistemaLivro.Infrastructure.Middlewares;
using QuestPDF.Infrastructure;
using Serilog;
using SistemaLivro.Api.Configuration;
using SistemaLivro.Api.Endpoints;

try
{
    var builder = WebApplication.CreateBuilder(args);
    QuestPDF.Settings.License = LicenseType.Community;

    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
        .AddEnvironmentVariables();

    builder.Services.AddRedisCache(builder.Configuration);
    builder.Services.AddRedisOutputCache(builder.Configuration);
    builder.Host.ConfigureSerilog(builder.Configuration);
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddApiConfig(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseCors("Development");
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseCors("Production");
        app.UseHsts();
    }

    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseCustomSerilogRequestLogging();

    app.UseRouting();
    app.UseAppConfig();
    app.UseOutputCache();
    app.MapControllers();
    app.MapEndpoints();

    app.UseSwaggerConfig();

    app.Run();

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

public partial class Program { }
