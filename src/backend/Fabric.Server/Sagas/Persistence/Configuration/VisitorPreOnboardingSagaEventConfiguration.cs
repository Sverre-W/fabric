using Fabric.Server.Sagas.VisitorPreOnboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Sagas.Persistence.Configuration;

public sealed class VisitorPreOnboardingSagaEventConfiguration : IEntityTypeConfiguration<VisitorPreOnboardingSagaEvent>
{
    public void Configure(EntityTypeBuilder<VisitorPreOnboardingSagaEvent> builder)
    {
        builder.ToTable("visitor_pre_onboarding_saga_events");

        builder.HasKey(x => x.Id).HasName("pk_visitor_pre_onboarding_saga_events");

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.SagaId).HasColumnName("saga_id");
        builder.Property(x => x.VisitId).HasColumnName("visit_id");
        builder.Property(x => x.InvitationId).HasColumnName("invitation_id");
        builder.Property(x => x.ArrivalId).HasColumnName("arrival_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.NextRetryAt).HasColumnName("next_retry_at");
        builder.Property(x => x.RetryCount).HasColumnName("retry_count").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(2000);

        builder.HasIndex(x => x.ProcessedAt).HasDatabaseName("ix_vpo_saga_events_processed_at");
        builder.HasIndex(x => x.NextRetryAt).HasDatabaseName("ix_vpo_saga_events_next_retry_at");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_vpo_saga_events_created_at");
    }
}
