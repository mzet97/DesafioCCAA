using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Infrastructure.Persistence;
using DesafioCCAA.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DesafioCCAA.Infrastructure.Configuration;

public static class IdentityConfig
{
    public static IServiceCollection AddWebIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureCommonIdentity(services, configuration);

        services
          .AddAuthentication(options =>
          {
              options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
              options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
          })
          .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
          {
              opts.LoginPath = "/Identity/Account/Login";
              opts.LogoutPath = "/Identity/Account/Logout";
              opts.AccessDeniedPath = "/Identity/Account/AccessDenied";
              opts.Cookie.Name = "DesafioCCAA.Auth";
              opts.ExpireTimeSpan = TimeSpan.FromHours(1);
              opts.SlidingExpiration = true;
          });

        return services;
    }


    public static IServiceCollection AddApiIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureCommonIdentity(services, configuration);

        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()
                          ?? throw new ApplicationException("AppSettings não encontrado");
        var key = Encoding.ASCII.GetBytes(appSettings.Secret);

        services
          .AddAuthentication(options =>
          {
              options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
              options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
          })
          .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
          {
              opts.RequireHttpsMetadata = true;
              opts.SaveToken = true;
              opts.TokenValidationParameters = new TokenValidationParameters
              {
                  ValidateIssuerSigningKey = true,
                  IssuerSigningKey = new SymmetricSecurityKey(key),
                  ValidateIssuer = true,
                  ValidIssuer = appSettings.Issuer,
                  ValidateAudience = true,
                  ValidAudience = appSettings.ValidOn,
                  ValidateLifetime = true
              };
          });

        return services;
    }

    private static void ConfigureCommonIdentity(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

        services.AddDbContext<ApplicationDbContext>((sp, opts) =>
        {
            var lf = sp.GetRequiredService<ILoggerFactory>();
            opts.UseLoggerFactory(lf)
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                opts.EnableSensitiveDataLogging();
                opts.EnableDetailedErrors();
            }
        });

        services
          .AddIdentity<ApplicationUser, ApplicationRole>(opts =>
          {
              opts.Password.RequireDigit = true;
              opts.Password.RequireLowercase = true;
              opts.Password.RequireNonAlphanumeric = true;
              opts.Password.RequireUppercase = true;
              opts.Password.RequiredLength = 6;
              opts.Password.RequiredUniqueChars = 1;
              opts.SignIn.RequireConfirmedAccount = true;
              opts.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*()_=?. ";
              opts.User.RequireUniqueEmail = true;
          })
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
    }
}