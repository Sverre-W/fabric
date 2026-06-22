using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.AccessPolicies.Persistence.Configuration;

public sealed class UsedBadgeNumberConfiguration : IEntityTypeConfiguration<UsedBadgeNumber>
{
    public void Configure(EntityTypeBuilder<UsedBadgeNumber> builder)
    {
        builder.ToTable("used_badge_numbers");

        builder.HasKey(number => number.Id).HasName("pk_used_badge_numbers");

        builder.Property(number => number.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(number => number.SystemId).HasColumnName("system_id").IsRequired();
        builder.Property(number => number.BadgeTypeId).HasColumnName("badge_type_id").IsRequired();
        builder.Property(number => number.SubjectId).HasColumnName("subject_id").IsRequired();
        builder.Property(number => number.BadgeNumber).HasColumnName("badge_number").IsRequired();

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(UsedBadgeNumber.SystemId), nameof(UsedBadgeNumber.BadgeTypeId), nameof(UsedBadgeNumber.BadgeNumber))
            .IsUnique()
            .HasDatabaseName("ix_used_badge_numbers_tenant_id_system_id_badge_type_id_badge_number");
    }
}
