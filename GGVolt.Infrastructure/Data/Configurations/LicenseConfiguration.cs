using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GGVolt.Core.Entities;

namespace GGVolt.Infrastructure.Data.Configurations;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.HasIndex(l => new { l.UserId, l.GameId }).IsUnique();
        builder.Property(l => l.Status).HasConversion<string>();
    }
}