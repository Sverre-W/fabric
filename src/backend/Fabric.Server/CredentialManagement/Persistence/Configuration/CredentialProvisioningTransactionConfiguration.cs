using Fabric.Server.CredentialManagement.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.CredentialManagement.Persistence.Configuration;

public sealed class CredentialProvisioningTransactionConfiguration : IEntityTypeConfiguration<CredentialProvisioningTransaction>
{
    public void Configure(EntityTypeBuilder<CredentialProvisioningTransaction> builder)
    {
        builder.ToTable("credential_provisioning_transactions");
        builder.HasKey(transaction => transaction.Id).HasName("pk_credential_provisioning_transactions");

        builder.Property(transaction => transaction.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(transaction => transaction.CredentialId).HasColumnName("credential_id").IsRequired();
        builder.Property(transaction => transaction.CredentialTypeTargetId).HasColumnName("credential_type_target_id").IsRequired();
        builder.Property(transaction => transaction.AccessControlSystemId).HasColumnName("access_control_system_id").IsRequired();
        builder.Property(transaction => transaction.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(transaction => transaction.ScheduledFor).HasColumnName("scheduled_for").IsRequired();
        builder.Property(transaction => transaction.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.Property(transaction => transaction.LastAttemptAt).HasColumnName("last_attempt_at");
        builder.Property(transaction => transaction.ProvisionedAt).HasColumnName("provisioned_at");
        builder.Property(transaction => transaction.RevokedAt).HasColumnName("revoked_at");
        builder.Property(transaction => transaction.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
        builder.Property(transaction => transaction.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(transaction => transaction.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasOne<Credential>()
            .WithMany()
            .HasForeignKey(transaction => transaction.CredentialId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CredentialTypeTarget>()
            .WithMany()
            .HasForeignKey(transaction => transaction.CredentialTypeTargetId)
            .OnDelete(DeleteBehavior.Restrict);

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialProvisioningTransaction.CredentialId))
            .HasDatabaseName("ix_credential_provisioning_transactions_tenant_id_credential_id");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialProvisioningTransaction.AccessControlSystemId), nameof(CredentialProvisioningTransaction.Status))
            .HasDatabaseName("ix_credential_provisioning_transactions_tenant_id_system_status");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(CredentialProvisioningTransaction.ScheduledFor))
            .HasDatabaseName("ix_credential_provisioning_transactions_tenant_id_scheduled_for");
    }
}
