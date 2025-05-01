using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Infrastructure.Email;
using DesafioCCAA.Infrastructure.Persistence;
using DesafioCCAA.Infrastructure.Redis;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DesafioCCAA.Infrastructure.Configuration;

public static class DependencyInjectionConfig
{
    public static IServiceCollection ResolveDependenciesInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<DbContext, ApplicationDbContext>();

        services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IRedisService, RedisService>();
        services.AddScoped<ISendEmail, SendEmail>();
        services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();

        return services;
    }
}
