using System.Text;
using BLL.Services;
using DAL.Context;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PL.Services;

namespace PL.Extensions;

public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection AddDalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                sqlite => sqlite.MigrationsAssembly("DAL")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddBllServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IResumeService,      ResumeService>();
        services.AddScoped<IVacancyService,     VacancyService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IUserService,        UserService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<JwtTokenService>();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer           = true,
                    ValidIssuer              = jwtSettings.Issuer,
                    ValidateAudience         = true,
                    ValidAudience            = jwtSettings.Audience,
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();

        return services;
    }
}
