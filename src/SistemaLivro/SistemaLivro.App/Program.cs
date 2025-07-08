using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using QuestPDF.Infrastructure;
using Serilog;
using SistemaLivro.App.Components;
using SistemaLivro.App.Components.Account;
using SistemaLivro.Application.Configuration;
using SistemaLivro.Infrastructure.Configuration;

try
{
    var builder = WebApplication.CreateBuilder(args);
    QuestPDF.Settings.License = LicenseType.Community;

    builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables();

    builder.Host.ConfigureSerilog(builder.Configuration);
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    builder.Services.AddWebIdentity(builder.Configuration);
    builder.Services.AddMessageBus(builder.Configuration);

    builder.Services.AddMudServices();

    // Adiciona cache distribuído em memória para IDistributedCache
    builder.Services.AddDistributedMemoryCache();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityUserAccessor>();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

    builder.Services.ResolveDependenciesApplication();
    builder.Services.ResolveDependenciesInfrastructure();
    builder.Services.AddApplicationServices();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();


    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.MapAdditionalIdentityEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Erro na aplicação: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

public partial class Program { }

