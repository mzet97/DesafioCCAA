using SistemaLivro.Domain.Domains.Identities;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLivro.Infrastructure.Email;
using SistemaLivro.Infrastructure.Persistence;
using SistemaLivro.Infrastructure.Redis;

namespace SistemaLivro.Infrastructure.Configuration;

public static class DependencyInjectionConfig
{
    public static IServiceCollection ResolveDependenciesInfrastructure(this IServiceCollection services)
    {

        services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IRedisService, RedisService>();
        services.AddScoped<ISendEmail, SendEmail>();
        services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();

        return services;
    }
}
