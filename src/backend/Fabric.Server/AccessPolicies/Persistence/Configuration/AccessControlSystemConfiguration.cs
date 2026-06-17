using Fabric.Server.AccessPolicies.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.AccessPolicies.Persistence.Configuration;

public sealed class AccessControlSystemConfiguration : IEntityTypeConfiguration<AccessControlSystem>
{
    public void Configure(EntityTypeBuilder<AccessControlSystem> builder)
    {
        builder.ToTable("access_control_systems");

        builder.HasKey(system => system.Id).HasName("pk_access_control_systems");

        builder.Property(system => system.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(system => system.Name).HasColumnName("name").IsRequired().HasMaxLength(200);

        builder.HasDiscriminator<string>("provider")
            .HasValue<UnipassAccessControlSystem>("unipass")
            .HasValue<LenelAccessControlSystem>("lenel");

        builder.HasIndex(system => system.Name).IsUnique().HasDatabaseName("ix_access_control_systems_name");
    }
}

public sealed class UnipassAccessControlSystemConfiguration : IEntityTypeConfiguration<UnipassAccessControlSystem>
{
    public void Configure(EntityTypeBuilder<UnipassAccessControlSystem> builder)
    {
        builder.OwnsOne(system => system.Config, config =>
        {
            config.Property(c => c.Endpoint).HasColumnName("unipass_endpoint").HasMaxLength(1_000);
            config.Property(c => c.SslValidation).HasColumnName("unipass_ssl_validation");
            config.Property(c => c.Username).HasColumnName("unipass_username").HasMaxLength(200);
            config.Property(c => c.Password).HasColumnName("unipass_password").HasMaxLength(2_000);
        });

        builder
            .HasMany(system => system.BadgeTypes)
            .WithOne()
            .HasForeignKey(badgeType => badgeType.SystemId)
            .HasConstraintName("fk_badge_types_access_control_systems_system_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(system => system.AccessLevels)
            .WithOne()
            .HasForeignKey(accessLevel => accessLevel.SystemId)
            .HasConstraintName("fk_access_level_types_access_control_systems_system_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class LenelAccessControlSystemConfiguration : IEntityTypeConfiguration<LenelAccessControlSystem>
{
    public void Configure(EntityTypeBuilder<LenelAccessControlSystem> builder)
    {
        builder.OwnsOne(system => system.Config, config =>
        {
            config.Property(c => c.Endpoint).HasColumnName("lenel_endpoint").HasMaxLength(1_000);
            config.Property(c => c.SslValidation).HasColumnName("lenel_ssl_validation");
            config.Property(c => c.ApiKey).HasColumnName("lenel_api_key").HasMaxLength(2_000);
        });

        builder
            .HasMany(system => system.BadgeTypes)
            .WithOne()
            .HasForeignKey(badgeType => badgeType.SystemId)
            .HasConstraintName("fk_badge_types_access_control_systems_system_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(system => system.AccessLevels)
            .WithOne()
            .HasForeignKey(accessLevel => accessLevel.SystemId)
            .HasConstraintName("fk_access_level_types_access_control_systems_system_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
