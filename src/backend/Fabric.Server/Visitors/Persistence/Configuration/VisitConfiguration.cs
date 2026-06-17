using Fabric.Server.Visitors.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Visitors.Persistence.Configuration;

public sealed class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("visits");

        builder.HasKey(visit => visit.Id).HasName("pk_visits");

        builder.Property(visit => visit.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(visit => visit.Summary).HasColumnName("summary").IsRequired().HasMaxLength(500);
        builder.Property(visit => visit.OrganizerId).HasColumnName("organizer_id").IsRequired();
        builder.Property(visit => visit.Status).HasColumnName("status").IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(visit => visit.Start).HasColumnName("start");
        builder.Property(visit => visit.Stop).HasColumnName("stop");
        builder.Property(visit => visit.LocationId).HasColumnName("location_id");

        builder
            .HasMany(visit => visit.Invitations)
            .WithOne()
            .HasForeignKey("visit_id")
            .HasConstraintName("fk_visit_invitations_visits_visit_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
