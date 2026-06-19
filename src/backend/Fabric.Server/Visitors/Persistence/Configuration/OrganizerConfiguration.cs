using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Visitors.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Visitors.Persistence.Configuration;

public sealed class OrganizerConfiguration : IEntityTypeConfiguration<Organizer>
{
    public void Configure(EntityTypeBuilder<Organizer> builder)
    {
        builder.ToTable("organizers");

        builder.HasKey(organizer => organizer.Id).HasName("pk_organizers");
        builder.Property(organizer => organizer.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(organizer => organizer.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(200);
        builder.Property(organizer => organizer.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(200);
        builder.Property(organizer => organizer.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
        builder.Property(organizer => organizer.Active).HasColumnName("active").IsRequired();

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(Organizer.Email))
            .IsUnique()
            .HasDatabaseName("ix_organizers_tenant_id_email");
    }
}
