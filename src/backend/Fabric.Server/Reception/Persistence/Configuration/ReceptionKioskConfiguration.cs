using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Reception.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Reception.Persistence.Configuration;

public sealed class ReceptionKioskConfiguration : IEntityTypeConfiguration<ReceptionKiosk>
{
    public void Configure(EntityTypeBuilder<ReceptionKiosk> builder)
    {
        builder.ToTable("reception_kiosks");

        builder.HasKey(kiosk => kiosk.Id).HasName("pk_reception_kiosks");

        builder.Property(kiosk => kiosk.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(kiosk => kiosk.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(kiosk => kiosk.LocationId).HasColumnName("location_id").IsRequired();
        builder.Property(kiosk => kiosk.ApiKeyHash).HasColumnName("api_key_hash").IsRequired().HasMaxLength(200);
        builder.Property(kiosk => kiosk.ApiKeySalt).HasColumnName("api_key_salt").IsRequired().HasMaxLength(200);
        builder.Property(kiosk => kiosk.Enabled).HasColumnName("enabled").IsRequired();

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(ReceptionKiosk.LocationId))
            .HasDatabaseName("ix_reception_kiosks_tenant_id_location_id");
    }
}
