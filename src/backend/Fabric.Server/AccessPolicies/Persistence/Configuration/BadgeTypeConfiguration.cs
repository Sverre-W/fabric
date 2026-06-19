using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.AccessPolicies.Persistence.Configuration;

public sealed class BadgeTypeConfiguration : IEntityTypeConfiguration<BadgeType>
{
    public void Configure(EntityTypeBuilder<BadgeType> builder)
    {
        builder.ToTable("badge_types");

        builder.HasKey(badgeType => badgeType.Id).HasName("pk_badge_types");

        builder.Property(badgeType => badgeType.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(badgeType => badgeType.SystemId).HasColumnName("system_id").IsRequired();
        builder.Property(badgeType => badgeType.Name).HasColumnName("name").IsRequired().HasMaxLength(200);

        builder.HasDiscriminator<string>("provider")
            .HasValue<UnipassBadgeType>("unipass")
            .HasValue<LenelBadgeType>("lenel");

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(badgeType => badgeType.SystemId).HasDatabaseName("ix_badge_types_system_id");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(BadgeType.SystemId), nameof(BadgeType.Name))
            .IsUnique()
            .HasDatabaseName("ix_badge_types_tenant_id_system_id_name");
    }
}

public sealed class UnipassBadgeTypeConfiguration : IEntityTypeConfiguration<UnipassBadgeType>
{
    public void Configure(EntityTypeBuilder<UnipassBadgeType> builder)
    {
        builder.OwnsOne(badgeType => badgeType.Range, range =>
        {
            range.Property(r => r.Start).HasColumnName("range_start").IsRequired();
            range.Property(r => r.Stop).HasColumnName("range_stop").IsRequired();
        });
    }
}

public sealed class LenelBadgeTypeConfiguration : IEntityTypeConfiguration<LenelBadgeType>
{
    public void Configure(EntityTypeBuilder<LenelBadgeType> builder)
    {
        builder.Property(badgeType => badgeType.BadgeTypeId).HasColumnName("badge_type_id").IsRequired();
        builder.HasIndex(
                TenantDbContext.TenantIdPropertyName,
                nameof(LenelBadgeType.SystemId),
                nameof(LenelBadgeType.BadgeTypeId))
            .IsUnique()
            .HasDatabaseName("ix_badge_types_tenant_id_system_id_badge_type_id");
    }
}
