using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GGVolt.Core.Entities;

namespace GGVolt.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(t => t.Currency).HasDefaultValue("RUB").HasMaxLength(3);
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.Amount).HasPrecision(18, 2);
    }
}