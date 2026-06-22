using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.AccessPolicies.Persistence.Configuration;

public sealed class IssuedProviderResourceConfiguration : IEntityTypeConfiguration<IssuedProviderResource>
{
    public void Configure(EntityTypeBuilder<IssuedProviderResource> builder)
    {
        builder.ToTable("issued_provider_resources");

        builder.HasKey(resource => resource.Id).HasName("pk_issued_provider_resources");

        builder.Property(resource => resource.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(resource => resource.PolicyId).HasColumnName("policy_id").IsRequired();
        builder.Property(resource => resource.SubjectId).HasColumnName("subject_id").IsRequired();
        builder.Property(resource => resource.SystemId).HasColumnName("system_id").IsRequired();
        builder.Property(resource => resource.ResourceKind)
            .HasColumnName("resource_kind")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        builder.Property(resource => resource.BadgeTypeId).HasColumnName("badge_type_id");
        builder.Property(resource => resource.BadgeNumber).HasColumnName("badge_number");
        builder.Property(resource => resource.AccessLevelTypeId).HasColumnName("access_level_type_id");
        builder.Property(resource => resource.ExternalPersonId).HasColumnName("external_person_id").IsRequired().HasMaxLength(500);
        builder.Property(resource => resource.ExternalResourceId).HasColumnName("external_resource_id").IsRequired().HasMaxLength(500);

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(resource => resource.PolicyId).HasDatabaseName("ix_issued_provider_resources_policy_id");
        builder.HasIndex(resource => new { resource.SubjectId, resource.SystemId }).HasDatabaseName("ix_issued_provider_resources_subject_id_system_id");
    }
}
