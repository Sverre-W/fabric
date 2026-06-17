using Fabric.Server.AccessPolicies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.AccessPolicies.Persistence.Configuration;

public sealed class IdentityMappingConfiguration : IEntityTypeConfiguration<IdentityMapping>
{
    public void Configure(EntityTypeBuilder<IdentityMapping> builder)
    {
        builder.ToTable("identity_mappings");

        builder.Property(mapping => mapping.SubjectId).HasColumnName("subject_id").ValueGeneratedNever();
        builder.Property(mapping => mapping.SystemId).HasColumnName("system_id").IsRequired();
        builder.Property(mapping => mapping.FirstName).HasColumnName("subject_first_name").IsRequired().HasMaxLength(200);
        builder.Property(mapping => mapping.LastName).HasColumnName("subject_last_name").IsRequired().HasMaxLength(200);
        builder.Property(mapping => mapping.SubjectType)
            .HasColumnName("subject_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        builder.Property(mapping => mapping.ExternalId).HasColumnName("external_id").IsRequired().HasMaxLength(500);

        builder.HasKey(mapping => new { mapping.SubjectId, mapping.SystemId }).HasName("pk_identity_mappings");
        builder.HasIndex(mapping => mapping.ExternalId).HasDatabaseName("ix_identity_mappings_external_id");
    }
}
