using Fabric.Server.AccessPolicies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.AccessPolicies.Persistence.Configuration;

public sealed class PolicyRequirementConfiguration : IEntityTypeConfiguration<PolicyRequirement>
{
    public void Configure(EntityTypeBuilder<PolicyRequirement> builder)
    {
        builder.ToTable("policy_requirements");

        builder.Property<Guid>("access_policy_id").HasColumnName("access_policy_id").ValueGeneratedNever();
        builder.HasKey("access_policy_id").HasName("pk_policy_requirements");

        builder.HasDiscriminator<string>("requirement_type")
            .HasValue<CredentialRequirement>("credential")
            .HasValue<AccessRequirement>("access");

        builder.Property<string>("requirement_type").HasColumnName("requirement_type").HasMaxLength(50);
    }
}

public sealed class CredentialRequirementConfiguration : IEntityTypeConfiguration<CredentialRequirement>
{
    public void Configure(EntityTypeBuilder<CredentialRequirement> builder)
    {
        builder.Property(requirement => requirement.BadgeNumber).HasColumnName("badge_number");
        builder
            .HasOne(requirement => requirement.BadgeType)
            .WithMany()
            .HasForeignKey("badge_type_id")
            .HasConstraintName("fk_policy_requirements_badge_types_badge_type_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AccessRequirementConfiguration : IEntityTypeConfiguration<AccessRequirement>
{
    public void Configure(EntityTypeBuilder<AccessRequirement> builder)
    {
        builder
            .HasOne(requirement => requirement.AccessLevel)
            .WithMany()
            .HasForeignKey("access_level_type_id")
            .HasConstraintName("fk_policy_requirements_access_level_types_access_level_type_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
