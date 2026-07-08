using Fabric.Server.Sagas.Kiosk;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Sagas.Persistence.Configuration;

public sealed class KioskSagaConfiguration : IEntityTypeConfiguration<KioskSaga>
{
    public void Configure(EntityTypeBuilder<KioskSaga> builder)
    {
        builder.ToTable("kiosk_sagas");
        builder.HasKey(x => x.Id).HasName("pk_kiosk_sagas");
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.SessionId).HasColumnName("session_id").IsRequired();
        builder.Property(x => x.WorkflowInstanceId).HasColumnName("workflow_instance_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CurrentInstructionId).HasColumnName("current_instruction_id").HasMaxLength(100);
        builder.Property(x => x.CurrentInstructionKind).HasColumnName("current_instruction_kind").HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.State).HasColumnName("state").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.HasIndex(x => x.SessionId).HasDatabaseName("ix_kiosk_sagas_session_id").IsUnique();
        builder.HasIndex(x => x.WorkflowInstanceId).HasDatabaseName("ix_kiosk_sagas_workflow_instance_id").IsUnique();
    }
}
