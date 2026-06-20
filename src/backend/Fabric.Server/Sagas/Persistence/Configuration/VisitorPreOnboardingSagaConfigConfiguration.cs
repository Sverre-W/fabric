using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Sagas.VisitorPreOnboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Sagas.Persistence.Configuration;

public sealed class VisitorPreOnboardingSagaConfigConfiguration : IEntityTypeConfiguration<VisitorPreOnboardingSagaConfig>
{
    public void Configure(EntityTypeBuilder<VisitorPreOnboardingSagaConfig> builder)
    {
        builder.ToTable("visitor_pre_onboarding_saga_configs");

        builder.HasKey(x => x.Id).HasName("pk_visitor_pre_onboarding_saga_configs");

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.UseCustomInviteNotification).HasColumnName("use_custom_invite_notification").IsRequired();
        builder.Property(x => x.CustomInviteNotification).HasColumnName("custom_invite_notification");
        builder.Property(x => x.QrGenerationMode).HasColumnName("qr_generation_mode").IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.SendConfirmNotificationToOrganizer).HasColumnName("send_confirm_notification_to_organizer").IsRequired();
        builder.Property(x => x.UseCustomConfirmNotification).HasColumnName("use_custom_confirm_notification").IsRequired();
        builder.Property(x => x.CustomConfirmNotification).HasColumnName("custom_confirm_notification");
        builder.Property(x => x.SendCancellationNotification).HasColumnName("send_cancellation_notification").IsRequired();
        builder.Property(x => x.UseCustomCancellationNotification).HasColumnName("use_custom_cancellation_notification").IsRequired();
        builder.Property(x => x.CustomCancellationNotification).HasColumnName("custom_cancellation_notification");
        builder.Property(x => x.SendRescheduleNotification).HasColumnName("send_reschedule_notification").IsRequired();
        builder.Property(x => x.UseCustomRescheduleNotification).HasColumnName("use_custom_reschedule_notification").IsRequired();
        builder.Property(x => x.CustomRescheduleNotification).HasColumnName("custom_reschedule_notification");

        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName)
            .IsUnique()
            .HasDatabaseName("ix_visitor_pre_onboarding_saga_configs_tenant_id");
    }
}
