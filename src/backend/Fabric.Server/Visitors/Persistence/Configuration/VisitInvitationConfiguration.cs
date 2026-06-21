using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Visitors.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Visitors.Persistence.Configuration;

public sealed class VisitInvitationConfiguration : IEntityTypeConfiguration<VisitInvitation>
{
    public void Configure(EntityTypeBuilder<VisitInvitation> builder)
    {
        builder.ToTable("visit_invitations");

        builder.HasKey(invitation => invitation.Id).HasName("pk_visit_invitations");

        builder.Property(invitation => invitation.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("visit_id").HasColumnName("visit_id");
        builder.Property(invitation => invitation.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(200);
        builder.Property(invitation => invitation.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(200);
        builder.Property(invitation => invitation.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
        builder.Property(invitation => invitation.Company).HasColumnName("company").IsRequired().HasMaxLength(200);
        builder.Property(invitation => invitation.ConfirmationStatus).HasColumnName("confirmation_status").IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(invitation => invitation.VisitorId).HasColumnName("visitor_id").IsRequired();
        builder.Property(invitation => invitation.RejectedAt).HasColumnName("rejected_at");
        builder.Property(invitation => invitation.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(invitation => invitation.Transport).HasColumnName("transport").HasConversion<string>().HasMaxLength(50);
        builder.Property(invitation => invitation.LicensePlate).HasColumnName("license_plate").HasMaxLength(50);

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, "visit_id", nameof(VisitInvitation.Email))
            .IsUnique()
            .HasDatabaseName("ix_visit_invitations_tenant_id_visit_id_email");
    }
}
