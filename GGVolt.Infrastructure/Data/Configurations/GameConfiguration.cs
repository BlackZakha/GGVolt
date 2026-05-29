using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GGVolt.Core.Entities;

namespace GGVolt.Infrastructure.Data.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.Property(g => g.Title).IsRequired().HasMaxLength(256);
        builder.Property(g => g.Description).HasMaxLength(4000);
        builder.Property(g => g.StorageKey).IsRequired().HasMaxLength(512);
        builder.Property(g => g.CoverUrl).HasMaxLength(1024);
        builder.Property(g => g.SystemRequirementsJson).HasColumnType("jsonb");
        builder.HasIndex(g => g.Title);
        builder.HasIndex(g => g.ContentType);
    }
}