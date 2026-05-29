using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GGVolt.Infrastructure.Data;
using GGVolt.Infrastructure.Repositories;
using GGVolt.Core.Entities;
using GGVolt.Infrastructure.Storage;

namespace GGVolt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") 
                               ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<GGVoltDbContext>(options =>
            options.UseNpgsql(connectionString)
                .EnableSensitiveDataLogging()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution));

        // Регистрируем IRepository<T> -> Repository<T>
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.ConfigureMinio(configuration);
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        
        return services;
    }
}