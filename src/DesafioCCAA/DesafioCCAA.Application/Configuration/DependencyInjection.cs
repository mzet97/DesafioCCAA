using DesafioCCAA.Application.Common.Behaviours;
using DesafioCCAA.Application.Services;
using DesafioCCAA.Domain.Services.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DesafioCCAA.Application.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        return services;
    }

    public static IServiceCollection ResolveDependenciesApplication(this IServiceCollection services)
    {
        services.AddScoped<IApplicationUserService, ApplicationUserService>();
        services.AddScoped<IRoleClaimService, RoleClaimService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserClaimService, UserClaimService>();
        services.AddScoped<IUserLoginService, UserLoginService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IUserTokenService, UserTokenService>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}

