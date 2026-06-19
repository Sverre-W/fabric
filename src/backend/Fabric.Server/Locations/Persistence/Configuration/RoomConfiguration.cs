using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Locations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Locations.Persistence.Configuration;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");

        builder.HasKey(room => room.Id).HasName("pk_rooms");

        builder.Property(room => room.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("building_id").HasColumnName("building_id").IsRequired();
        builder.Property(room => room.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(room => room.Capacity).HasColumnName("capacity").IsRequired();
        builder.Property(room => room.WheelchairAccessible).HasColumnName("wheelchair_accessible").IsRequired();

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, "building_id", nameof(Room.Name))
            .IsUnique()
            .HasDatabaseName("ix_rooms_tenant_id_building_id_name");
    }
}
