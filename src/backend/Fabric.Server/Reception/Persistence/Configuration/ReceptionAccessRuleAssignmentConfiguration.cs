using Fabric.Server.Reception.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Reception.Persistence.Configuration;

public sealed class ReceptionAccessRuleAssignmentConfiguration : IEntityTypeConfiguration<ReceptionAccessRuleAssignment>
{
    public void Configure(EntityTypeBuilder<ReceptionAccessRuleAssignment> builder)
    {
        builder.ToTable("access_rule_assignments");

        builder.HasKey(assignment => assignment.Id).HasName("pk_access_rule_assignments");

        builder.Property(assignment => assignment.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(assignment => assignment.LocationId).HasColumnName("location_id").IsRequired();
        builder.Property(assignment => assignment.SystemId).HasColumnName("system_id").IsRequired();
        builder.Property(assignment => assignment.AccessLevelTypeId).HasColumnName("access_level_type_id").IsRequired();
        builder.Property(assignment => assignment.GracePeriodMinutes).HasColumnName("grace_period_minutes").IsRequired();
        builder.Property(assignment => assignment.Trigger)
            .HasColumnName("trigger")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(assignment => assignment.LocationId).HasDatabaseName("ix_access_rule_assignments_location_id");
        builder.HasIndex(assignment => assignment.SystemId).HasDatabaseName("ix_access_rule_assignments_system_id");
        builder.HasIndex(assignment => assignment.AccessLevelTypeId).HasDatabaseName("ix_access_rule_assignments_access_level_type_id");
        builder.HasIndex(assignment => assignment.Trigger).HasDatabaseName("ix_access_rule_assignments_trigger");
    }
}
