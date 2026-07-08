using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Kiosk.Persistence.Configuration;

public sealed class KioskConfiguration : IEntityTypeConfiguration<Domain.Kiosk>
{
    public void Configure(EntityTypeBuilder<Domain.Kiosk> builder)
    {
        builder.ToTable("kiosks");
        builder.HasKey(kiosk => kiosk.Id).HasName("pk_kiosks");
        builder.Property(kiosk => kiosk.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(kiosk => kiosk.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(kiosk => kiosk.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(kiosk => kiosk.Mode).HasColumnName("mode").HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(kiosk => kiosk.ApiKeyHash).HasColumnName("api_key_hash").IsRequired().HasMaxLength(200);
        builder.Property(kiosk => kiosk.ApiKeySalt).HasColumnName("api_key_salt").IsRequired().HasMaxLength(200);
        builder.Property(kiosk => kiosk.WorkflowDefinitionId).HasColumnName("workflow_definition_id").HasMaxLength(200);
        builder.Property(kiosk => kiosk.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(kiosk => kiosk.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(kiosk => kiosk.UpdatedAt).HasColumnName("updated_at").IsRequired();
        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(Domain.Kiosk.Name)).HasDatabaseName("ix_kiosks_tenant_id_name").IsUnique();
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(Domain.Kiosk.ProfileId)).HasDatabaseName("ix_kiosks_tenant_id_profile_id");
    }
}
