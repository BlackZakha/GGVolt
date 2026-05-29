using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GGVolt.Infrastructure.Storage;

public static class StorageOptionsExtensions
{
    public static IServiceCollection ConfigureMinio(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<MinioSettings>(
            configuration.GetSection(MinioSettings.Section));
        
        return services;
    }
}