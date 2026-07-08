using Fabric.Server.Desfire.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Desfire.Persistence.Configuration;

public sealed class KeyDiversificationStrategyConfiguration : IEntityTypeConfiguration<KeyDiversificationStrategyEntity>
{
    public void Configure(EntityTypeBuilder<KeyDiversificationStrategyEntity> builder)
    {
        builder.ToTable("key_diversification_strategies");
        builder.HasKey(strategy => strategy.Id);
        builder.Property(strategy => strategy.Id).ValueGeneratedNever();
        builder.Property(strategy => strategy.Name).IsRequired().HasMaxLength(200);
        builder.Property(strategy => strategy.Algorithm).HasConversion<string>().IsRequired().HasMaxLength(80);
        builder.Property(strategy => strategy.InputsJson).IsRequired().HasColumnType("jsonb");
        builder.Property(strategy => strategy.CreatedAt).IsRequired();
        builder.Property(strategy => strategy.UpdatedAt).IsRequired();
        builder.HasIndex(strategy => strategy.Name).IsUnique();
    }
}
