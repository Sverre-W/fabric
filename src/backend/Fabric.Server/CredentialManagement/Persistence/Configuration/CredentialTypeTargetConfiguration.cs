using Fabric.Server.CredentialManagement.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.CredentialManagement.Persistence.Configuration;

public sealed class CredentialTypeTargetConfiguration : IEntityTypeConfiguration<CredentialTypeTarget>
{
    public void Configure(EntityTypeBuilder<CredentialTypeTarget> builder)
    {
        builder.ToTable("credential_type_targets");
        builder.HasKey(target => target.Id).HasName("pk_credential_type_targets");

        builder.Property(target => target.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(target => target.CredentialTypeId).HasColumnName("credential_type_id").IsRequired();
        builder.Property(target => target.AccessControlSystemId).HasColumnName("access_control_system_id").IsRequired();
        builder.Property(target => target.ProviderCredentialTypeId).HasColumnName("provider_credential_type_id");
        builder.Property(target => target.ProvisioningTiming).HasColumnName("provisioning_timing").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(target => target.IsEnabled).HasColumnName("is_enabled").IsRequired();
        builder.Property(target => target.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(target => target.UpdatedAt).HasColumnName("updated_at").IsRequired();

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialTypeTarget.CredentialTypeId), nameof(CredentialTypeTarget.AccessControlSystemId))
            .IsUnique()
            .HasDatabaseName("ix_credential_type_targets_tenant_id_type_system");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialTypeTarget.AccessControlSystemId))
            .HasDatabaseName("ix_credential_type_targets_tenant_id_system");
    }
}
