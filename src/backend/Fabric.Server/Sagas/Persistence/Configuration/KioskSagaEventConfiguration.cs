using Fabric.Server.Sagas.Kiosk;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Sagas.Persistence.Configuration;

public sealed class KioskSagaEventConfiguration : IEntityTypeConfiguration<KioskSagaEvent>
{
    public void Configure(EntityTypeBuilder<KioskSagaEvent> builder)
    {
        builder.ToTable("kiosk_saga_events");
        builder.HasKey(x => x.Id).HasName("pk_kiosk_saga_events");
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.SagaId).HasColumnName("saga_id").IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.InstructionId).HasColumnName("instruction_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.InstructionKind).HasColumnName("instruction_kind").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.ResultJson).HasColumnName("result_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.NextRetryAt).HasColumnName("next_retry_at");
        builder.Property(x => x.RetryCount).HasColumnName("retry_count").IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(2000);
        builder.HasIndex(x => x.ProcessedAt).HasDatabaseName("ix_kiosk_saga_events_processed_at");
        builder.HasIndex(x => x.NextRetryAt).HasDatabaseName("ix_kiosk_saga_events_next_retry_at");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_kiosk_saga_events_created_at");
    }
}
