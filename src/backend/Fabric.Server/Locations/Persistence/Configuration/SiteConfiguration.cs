using Fabric.Server.Locations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Locations.Persistence.Configuration;

public sealed class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("sites");

        builder.HasKey(site => site.Id).HasName("pk_sites");

        builder.Property(site => site.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(site => site.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(site => site.Address).HasColumnName("address").HasMaxLength(500);

        builder
            .HasMany(site => site.Buildings)
            .WithOne()
            .HasForeignKey("site_id")
            .HasConstraintName("fk_buildings_sites_site_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
