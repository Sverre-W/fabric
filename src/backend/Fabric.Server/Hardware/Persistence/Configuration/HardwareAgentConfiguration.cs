using Fabric.Server.Hardware.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Hardware.Persistence.Configuration;

public sealed class HardwareAgentConfiguration : IEntityTypeConfiguration<HardwareAgent>
{
    public void Configure(EntityTypeBuilder<HardwareAgent> builder)
    {
        builder.ToTable("agents");
        builder.HasKey(agent => agent.Id);
        builder.Property(agent => agent.Id).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(agent => agent.Name).IsRequired().HasMaxLength(200);
        builder.Property(agent => agent.Enabled).IsRequired();
        builder.Property(agent => agent.ApiKeyHash).IsRequired().HasMaxLength(500);
        builder.Property(agent => agent.ApiKeySalt).IsRequired().HasMaxLength(200);
    }
}
