using Fabric.Server.Desfire.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Desfire.Persistence.Configuration;

public sealed class DesfireSystemProviderConfiguration : IEntityTypeConfiguration<DesfireSystemProvider>
{
    public void Configure(EntityTypeBuilder<DesfireSystemProvider> builder)
    {
        builder.ToTable("system_providers");
        builder.HasKey(provider => provider.Id);
        builder.Property(provider => provider.Id).ValueGeneratedNever();
        builder.Property(provider => provider.Name).IsRequired().HasMaxLength(200);
        builder.Property(provider => provider.ProviderType).IsRequired();
        builder.Property(provider => provider.FixedValue).HasMaxLength(2000);
        builder.Property(provider => provider.CreatedAt).IsRequired();
        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(DesfireSystemProvider.Name))
            .IsUnique()
            .HasDatabaseName("ix_system_providers_tenant_id_name");
    }
}
