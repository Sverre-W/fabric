using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Locations.Persistence.Configuration;

public sealed class LocationLookupConfiguration : IEntityTypeConfiguration<LocationLookup>
{
    public void Configure(EntityTypeBuilder<LocationLookup> builder)
    {
        builder.ToTable("location_lookup");

        builder.HasKey(lookup => lookup.Id).HasName("pk_location_lookup");

        builder.Property(lookup => lookup.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(lookup => lookup.Type).HasColumnName("type").IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(lookup => lookup.SiteId).HasColumnName("site_id").IsRequired();
        builder.Property(lookup => lookup.BuildingId).HasColumnName("building_id");
        builder.Property(lookup => lookup.RoomId).HasColumnName("room_id");

        builder.HasIndex(lookup => lookup.SiteId).HasDatabaseName("ix_location_lookup_site_id");
        builder.HasIndex(lookup => lookup.BuildingId).HasDatabaseName("ix_location_lookup_building_id");
        builder.HasIndex(lookup => lookup.RoomId).HasDatabaseName("ix_location_lookup_room_id");
    }
}
