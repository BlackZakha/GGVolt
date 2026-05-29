using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GGVolt.Infrastructure.Data;

/// Фабрика для инструментов EF Core (migrations, database update).
/// Используется ТОЛЬКО на этапе разработки.

public class GGVoltDbContextFactory : IDesignTimeDbContextFactory<GGVoltDbContext>
{
    public GGVoltDbContext CreateDbContext(string[] args)
    {
        // 1. Собираем конфигурацию вручную (ищем appsettings.json в проекте сервера)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../GGVolt.Server"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // 2. Получаем строку подключения
        var connectionString = configuration.GetConnectionString("Default") 
                               ?? "Host=localhost;Database=ggvolt_dev;Username=postgres;Password=dev_pass_123";

        // 3. Настраиваем опции для БД
        var optionsBuilder = new DbContextOptionsBuilder<GGVoltDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("GGVolt.Infrastructure"));

        return new GGVoltDbContext(optionsBuilder.Options);
    }
}