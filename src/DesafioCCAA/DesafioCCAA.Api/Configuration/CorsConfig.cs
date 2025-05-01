using DesafioCCAA.Shared.Settings;

namespace DesafioCCAA.Api.Configuration;

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
