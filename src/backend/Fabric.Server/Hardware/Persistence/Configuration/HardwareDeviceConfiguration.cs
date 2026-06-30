using Fabric.Server.Hardware.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Hardware.Persistence.Configuration;

public sealed class HardwareDeviceConfiguration : IEntityTypeConfiguration<HardwareDevice>
{
    public void Configure(EntityTypeBuilder<HardwareDevice> builder)
    {
        builder.ToTable("devices");
        builder.HasKey(device => device.Id);
        builder.Property(device => device.Id).ValueGeneratedNever();
        builder.Property(device => device.AgentId).IsRequired().HasMaxLength(100);
        builder.Property(device => device.DeviceId).IsRequired().HasMaxLength(100);
        builder.Property(device => device.Kind).IsRequired().HasMaxLength(100);
        builder.Property(device => device.Driver).IsRequired().HasMaxLength(100);
        builder.Property(device => device.Capabilities).IsRequired();
        builder.Property(device => device.State).IsRequired().HasMaxLength(100);
        builder.Property(device => device.Enabled).IsRequired();
        builder.Property(device => device.DiagnosticsJson).IsRequired().HasColumnType("jsonb");
        builder.HasIndex(device => new { device.AgentId, device.DeviceId }).IsUnique();
    }
}
