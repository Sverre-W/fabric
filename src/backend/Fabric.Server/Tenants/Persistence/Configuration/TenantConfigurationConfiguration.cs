using Fabric.Server.Tenants.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Tenants.Persistence.Configuration;

public sealed class TenantConfigurationConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(tenant => tenant.Id).HasName("pk_tenants");
        builder.Property(tenant => tenant.Id).HasColumnName("id").HasMaxLength(100).ValueGeneratedNever();

        builder.OwnsOne(tenant => tenant.Configuration, configuration =>
        {
            configuration.OwnsOne(c => c.Oidc, oidc =>
            {
                oidc.Property(o => o.MetadataUrl)
                    .HasColumnName("oidc_metadata_url")
                    .HasMaxLength(2_000)
                    .IsRequired();

                oidc.Property(o => o.ClientId)
                    .HasColumnName("oidc_client_id")
                    .HasMaxLength(200)
                    .IsRequired();

                oidc.Property(o => o.RequireHttpsMetadata)
                    .HasColumnName("oidc_require_https_metadata")
                    .IsRequired();
            });
        });
    }
}
