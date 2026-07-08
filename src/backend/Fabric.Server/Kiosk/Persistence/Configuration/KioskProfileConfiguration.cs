using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Kiosk.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Kiosk.Persistence.Configuration;

public sealed class KioskProfileConfiguration : IEntityTypeConfiguration<KioskProfile>
{
    public void Configure(EntityTypeBuilder<KioskProfile> builder)
    {
        builder.ToTable("profiles");
        builder.HasKey(profile => profile.Id).HasName("pk_kiosk_profiles");
        builder.Property(profile => profile.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(profile => profile.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(profile => profile.DefaultLanguageCode).HasColumnName("default_language_code").IsRequired().HasMaxLength(20);
        builder.Property(profile => profile.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(profile => profile.UpdatedAt).HasColumnName("updated_at").IsRequired();
        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(KioskProfile.Name)).HasDatabaseName("ix_kiosk_profiles_tenant_id_name").IsUnique();
    }
}
