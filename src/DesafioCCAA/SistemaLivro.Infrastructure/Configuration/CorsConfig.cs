using SistemaLivro.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SistemaLivro.Infrastructure.Configuration;

public static class CorsConfig
{
    public static IServiceCollection AddCorsConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOptions = new CorsOptions();
        configuration.GetSection("Cors").Bind(corsOptions);

        services.AddCors(options =>
        {
            options.AddPolicy("Development",
                builder =>
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

            options.AddPolicy("Production",
                builder =>
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        return services;
    }
}
