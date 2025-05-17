using SistemaLivro.Application.Configuration;
using SistemaLivro.Infrastructure.Configuration;

namespace SistemaLivro.Api.Configuration;

public static class ApiConfig
{
    public static IServiceCollection AddApiConfig(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddApiIdentity(configuration);
        services.AddCorsConfig(configuration);
        services.ResolveDependenciesInfrastructure();
        services.ResolveDependenciesApplication();

        services.AddMessageBus(configuration);
        services.AddApplicationServices();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfig();
        services.AddSwaggerGen();
        services.AddHealthChecks();
        services.AddAuthorization();
        services.AddApiResponseCompression();

        services.AddControllers();

        return services;
    }
}
