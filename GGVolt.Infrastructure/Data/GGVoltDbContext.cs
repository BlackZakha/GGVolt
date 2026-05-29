using Microsoft.EntityFrameworkCore;
using GGVolt.Core.Entities;

namespace GGVolt.Infrastructure.Data;

public class GGVoltDbContext : DbContext
{
    public GGVoltDbContext(DbContextOptions<GGVoltDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Автоматически подхватывает все IEntityTypeConfiguration из этой сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GGVoltDbContext).Assembly);
        
        // Глобальный фильтр для мягкого удаления (опционально)
        // modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        
        base.OnModelCreating(modelBuilder);
    }

    // Автоматическое проставление UpdatedAt
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        return base.SaveChangesAsync(cancellationToken);
    }
}