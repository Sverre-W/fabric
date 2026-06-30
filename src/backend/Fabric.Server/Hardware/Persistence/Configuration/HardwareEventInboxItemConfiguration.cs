using Fabric.Server.Hardware.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Hardware.Persistence.Configuration;

public sealed class HardwareEventInboxItemConfiguration : IEntityTypeConfiguration<HardwareEventInboxItem>
{
    public void Configure(EntityTypeBuilder<HardwareEventInboxItem> builder)
    {
        builder.ToTable("event_inbox");
        builder.HasKey(item => item.EventId);
        builder.Property(item => item.EventId).ValueGeneratedNever();
        builder.Property(item => item.AgentId).IsRequired().HasMaxLength(100);
        builder.Property(item => item.DeviceId).IsRequired().HasMaxLength(100);
        builder.Property(item => item.Type).IsRequired().HasMaxLength(100);
        builder.Property(item => item.PayloadJson).IsRequired().HasColumnType("jsonb");
    }
}
