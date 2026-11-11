using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VulnTracker.Application.Interfaces;
using VulnTracker.Infrastructure.Database;
using VulnTracker.Infrastructure.Repositories;
using VulnTracker.Infrastructure.Security;

namespace VulnTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(config.GetConnectionString("Default")));
        services.AddScoped<IFindingRepository, FindingRepository>();
        services.AddSingleton<FieldEncryptor>();
        return services;
    }
}