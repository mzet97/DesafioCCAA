using SistemaLivro.Application.Configuration;
using SistemaLivro.Infrastructure.Configuration;
using QuestPDF.Infrastructure;
using Serilog;

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

    if (builder.Environment.IsDevelopment())
    {
        builder.Services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation();

        builder.Services
            .AddRazorPages()
            .AddRazorRuntimeCompilation();
    }
    else
    {
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
    }

    // Adicione as configurações necessárias
    builder.Services.ResolveDependenciesApplication();
    builder.Services.ResolveDependenciesInfrastructure();
    builder.Services.AddApplicationServices();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        //app.UseHsts();
    }

    //app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapRazorPages()
       .WithStaticAssets();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

public partial class Program { }