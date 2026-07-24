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
        builder.Property(type => type.AllocationMode).HasColumnName("allocation_mode").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(type => type.NearLimitThreshold).HasColumnName("near_limit_threshold");
        builder.Property(type => type.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(type => type.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(type => type.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasMany(type => type.Ranges)
            .WithOne()
            .HasForeignKey(range => range.CredentialTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(CredentialType.Ranges))!.SetPropertyAccessMode(PropertyAccessMode.Field);

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialType.Name))
            .IsUnique()
            .HasDatabaseName("ix_credential_types_tenant_id_name");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialType.Status))
            .HasDatabaseName("ix_credential_types_tenant_id_status");
    }
}

public sealed class CredentialRangeConfiguration : IEntityTypeConfiguration<CredentialRange>
{
    public void Configure(EntityTypeBuilder<CredentialRange> builder)
    {
        builder.ToTable("credential_ranges");
        builder.HasKey(range => range.Id).HasName("pk_credential_ranges");

        builder.Property(range => range.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(range => range.CredentialTypeId).HasColumnName("credential_type_id").IsRequired();
        builder.Property(range => range.RangeStart).HasColumnName("range_start").IsRequired();
        builder.Property(range => range.RangeStop).HasColumnName("range_stop").IsRequired();
        builder.Property(range => range.IsActive).HasColumnName("is_active").IsRequired();

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialRange.CredentialTypeId))
            .HasDatabaseName("ix_credential_ranges_tenant_id_credential_type_id");
    }
}
