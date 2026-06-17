using Fabric.Server.AccessPolicies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.AccessPolicies.Persistence.Configuration;

public sealed class AccessLevelTypeConfiguration : IEntityTypeConfiguration<AccessLevelType>
{
    public void Configure(EntityTypeBuilder<AccessLevelType> builder)
    {
        builder.ToTable("access_level_types");

        builder.HasKey(accessLevel => accessLevel.Id).HasName("pk_access_level_types");

        builder.Property(accessLevel => accessLevel.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(accessLevel => accessLevel.SystemId).HasColumnName("system_id").IsRequired();
        builder.Property(accessLevel => accessLevel.Name).HasColumnName("name").IsRequired().HasMaxLength(200);

        builder.HasDiscriminator<string>("provider")
            .HasValue<UnipassAccessLevelType>("unipass")
            .HasValue<LenelAccessLevelType>("lenel");

        builder.HasIndex(accessLevel => accessLevel.SystemId).HasDatabaseName("ix_access_level_types_system_id");
        builder.HasIndex(accessLevel => new { accessLevel.SystemId, accessLevel.Name })
            .IsUnique()
            .HasDatabaseName("ix_access_level_types_system_id_name");
    }
}

public sealed class UnipassAccessLevelTypeConfiguration : IEntityTypeConfiguration<UnipassAccessLevelType>
{
    public void Configure(EntityTypeBuilder<UnipassAccessLevelType> builder)
    {
        builder.Property(accessLevel => accessLevel.SiteId).HasColumnName("site_id").IsRequired();
        builder.Property(accessLevel => accessLevel.AccessRuleId).HasColumnName("access_rule_id").IsRequired();
        builder.HasIndex(accessLevel => new { accessLevel.SystemId, accessLevel.SiteId, accessLevel.AccessRuleId })
            .IsUnique()
            .HasDatabaseName("ix_access_level_types_system_id_site_id_access_rule_id");
    }
}

public sealed class LenelAccessLevelTypeConfiguration : IEntityTypeConfiguration<LenelAccessLevelType>
{
    public void Configure(EntityTypeBuilder<LenelAccessLevelType> builder)
    {
        builder.Property(accessLevel => accessLevel.AccessLevelId).HasColumnName("access_level_id").IsRequired();

        builder
            .HasMany(accessLevel => accessLevel.BadgeTypes)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "lenel_access_level_type_badge_types",
                right => right
                    .HasOne<LenelBadgeType>()
                    .WithMany()
                    .HasForeignKey("badge_type_id")
                    .HasConstraintName("fk_lenel_access_level_type_badge_types_badge_types_badge_type_id")
                    .OnDelete(DeleteBehavior.Restrict),
                left => left
                    .HasOne<LenelAccessLevelType>()
                    .WithMany()
                    .HasForeignKey("access_level_type_id")
                    .HasConstraintName("fk_lenel_access_level_type_badge_types_access_level_types_access_level_type_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("lenel_access_level_type_badge_types");
                    join.HasKey("access_level_type_id", "badge_type_id")
                        .HasName("pk_lenel_access_level_type_badge_types");
                    join.Property<Guid>("access_level_type_id").HasColumnName("access_level_type_id");
                    join.Property<Guid>("badge_type_id").HasColumnName("badge_type_id");
                });

        builder.HasIndex(accessLevel => new { accessLevel.SystemId, accessLevel.AccessLevelId })
            .IsUnique()
            .HasDatabaseName("ix_access_level_types_system_id_access_level_id");
    }
}
