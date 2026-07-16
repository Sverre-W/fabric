using Fabric.Server.CredentialManagement.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.CredentialManagement.Persistence.Configuration;

public sealed class CredentialTypeConfiguration : IEntityTypeConfiguration<CredentialType>
{
    public void Configure(EntityTypeBuilder<CredentialType> builder)
    {
        builder.ToTable("credential_types");
        builder.HasKey(type => type.Id).HasName("pk_credential_types");

        builder.Property(type => type.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(type => type.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(type => type.Technology).HasColumnName("technology").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(type => type.RangeStart).HasColumnName("range_start").IsRequired();
        builder.Property(type => type.RangeStop).HasColumnName("range_stop").IsRequired();
        builder.Property(type => type.NearLimitThreshold).HasColumnName("near_limit_threshold");
        builder.Property(type => type.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(type => type.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(type => type.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasMany(type => type.Targets)
            .WithOne()
            .HasForeignKey(target => target.CredentialTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(CredentialType.Targets))!.SetPropertyAccessMode(PropertyAccessMode.Field);

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialType.Name))
            .IsUnique()
            .HasDatabaseName("ix_credential_types_tenant_id_name");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialType.Technology))
            .HasDatabaseName("ix_credential_types_tenant_id_technology");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialType.Status))
            .HasDatabaseName("ix_credential_types_tenant_id_status");
    }
}
