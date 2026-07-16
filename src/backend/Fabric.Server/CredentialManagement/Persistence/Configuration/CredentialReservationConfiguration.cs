using Fabric.Server.CredentialManagement.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.CredentialManagement.Persistence.Configuration;

public sealed class CredentialReservationConfiguration : IEntityTypeConfiguration<CredentialReservation>
{
    public void Configure(EntityTypeBuilder<CredentialReservation> builder)
    {
        builder.ToTable("credential_reservations");
        builder.HasKey(reservation => reservation.Id).HasName("pk_credential_reservations");

        builder.Property(reservation => reservation.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(reservation => reservation.CredentialTypeId).HasColumnName("credential_type_id").IsRequired();
        builder.Property(reservation => reservation.CredentialNumber).HasColumnName("credential_number").IsRequired();
        builder.Property(reservation => reservation.IdentityId).HasColumnName("identity_id").IsRequired();
        builder.Property(reservation => reservation.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(reservation => reservation.Purpose).HasColumnName("purpose").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(reservation => reservation.SourceKind).HasColumnName("source_kind").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(reservation => reservation.SourceId).HasColumnName("source_id");
        builder.Property(reservation => reservation.RequestedByIdentityId).HasColumnName("requested_by_identity_id");
        builder.Property(reservation => reservation.ReasonText).HasColumnName("reason_text").HasMaxLength(1000).IsRequired();
        builder.Property(reservation => reservation.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(reservation => reservation.ConsumedAt).HasColumnName("consumed_at");
        builder.Property(reservation => reservation.ReleasedAt).HasColumnName("released_at");
        builder.Property(reservation => reservation.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(reservation => reservation.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasOne<CredentialType>()
            .WithMany()
            .HasForeignKey(reservation => reservation.CredentialTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialReservation.CredentialTypeId), nameof(CredentialReservation.CredentialNumber))
            .IsUnique()
            .HasFilter("status = 'Active'")
            .HasDatabaseName("ix_credential_reservations_tenant_id_active_number");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialReservation.IdentityId))
            .HasDatabaseName("ix_credential_reservations_tenant_id_identity_id");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialReservation.SourceKind), nameof(CredentialReservation.SourceId))
            .HasDatabaseName("ix_credential_reservations_tenant_id_source");
    }
}
