using Fabric.Server.Desfire.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Desfire.Persistence.Configuration;

public sealed class DesfireEncoderConfiguration : IEntityTypeConfiguration<DesfireEncoder>
{
    public void Configure(EntityTypeBuilder<DesfireEncoder> builder)
    {
        builder.ToTable("encoders");
        builder.HasKey(encoder => encoder.Id);
        builder.Property(encoder => encoder.Id).ValueGeneratedNever();
        builder.Property(encoder => encoder.Name).IsRequired().HasMaxLength(200);
        builder.Property(encoder => encoder.AgentId).IsRequired().HasMaxLength(100);
        builder.Property(encoder => encoder.DeviceId).IsRequired().HasMaxLength(100);
        builder.Property(encoder => encoder.SupportsEncoding).IsRequired();
        builder.Property(encoder => encoder.SupportsPrinting).IsRequired();
        builder.Property(encoder => encoder.Enabled).IsRequired();
        builder.Property(encoder => encoder.CreatedAt).IsRequired();
        builder.Property(encoder => encoder.UpdatedAt).IsRequired();
        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(DesfireEncoder.Name)).IsUnique().HasDatabaseName("ix_encoders_tenant_id_name");
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(DesfireEncoder.AgentId), nameof(DesfireEncoder.DeviceId)).IsUnique().HasDatabaseName("ix_encoders_tenant_id_agent_id_device_id");
    }
}
