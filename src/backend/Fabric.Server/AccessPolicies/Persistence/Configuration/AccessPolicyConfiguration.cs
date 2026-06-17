using Fabric.Server.AccessPolicies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.AccessPolicies.Persistence.Configuration;

public sealed class AccessPolicyConfiguration : IEntityTypeConfiguration<AccessPolicy>
{
    public void Configure(EntityTypeBuilder<AccessPolicy> builder)
    {
        builder.ToTable("access_policies");

        builder.HasKey(policy => policy.Id).HasName("pk_access_policies");

        builder.Property(policy => policy.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(policy => policy.SystemId).HasColumnName("system_id").IsRequired();
        builder.Property(policy => policy.EffectiveFrom).HasColumnName("effective_from").IsRequired();
        builder.Property(policy => policy.EffectiveUntil).HasColumnName("effective_until").IsRequired();
        builder.Property(policy => policy.ReconciliationStatus)
            .HasColumnName("reconciliation_status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        builder.Property(policy => policy.ReconciliationFailureReason)
            .HasColumnName("reconciliation_failure_reason")
            .HasMaxLength(2_000);

        builder.OwnsOne(policy => policy.Subject, subject =>
        {
            subject.Property(s => s.Id).HasColumnName("subject_id").ValueGeneratedNever();
            subject.Property(s => s.FirstName).HasColumnName("subject_first_name").IsRequired().HasMaxLength(200);
            subject.Property(s => s.LastName).HasColumnName("subject_last_name").IsRequired().HasMaxLength(200);
            subject.Property(s => s.SubjectType)
                .HasColumnName("subject_type")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        builder
            .HasOne(policy => policy.Requirement)
            .WithOne()
            .HasForeignKey<PolicyRequirement>("access_policy_id")
            .HasConstraintName("fk_policy_requirements_access_policies_access_policy_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(policy => policy.SystemId).HasDatabaseName("ix_access_policies_system_id");
        builder.HasIndex(policy => policy.EffectiveUntil).HasDatabaseName("ix_access_policies_effective_until");
        builder.HasIndex(policy => policy.ReconciliationStatus).HasDatabaseName("ix_access_policies_reconciliation_status");
    }
}
