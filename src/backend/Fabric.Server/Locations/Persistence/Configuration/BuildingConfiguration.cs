using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Locations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Locations.Persistence.Configuration;

public sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("buildings");

        builder.HasKey(building => building.Id).HasName("pk_buildings");

        builder.Property(building => building.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("site_id").HasColumnName("site_id").IsRequired();
        builder.Property(building => building.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(building => building.Address).HasColumnName("address").HasMaxLength(500);

        builder
            .HasMany(building => building.Rooms)
            .WithOne()
            .HasForeignKey("building_id")
            .HasConstraintName("fk_rooms_buildings_building_id")
            .OnDelete(DeleteBehavior.Cascade);

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, "site_id", nameof(Building.Name))
            .IsUnique()
            .HasDatabaseName("ix_buildings_tenant_id_site_id_name");
    }
}
