using Fabric.Server.Desfire.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Desfire.Persistence.Configuration;

public sealed class EncodingBatchConfiguration : IEntityTypeConfiguration<EncodingBatch>
{
    public void Configure(EntityTypeBuilder<EncodingBatch> builder)
    {
        builder.ToTable("encoding_batches");
        builder.HasKey(batch => batch.Id);
        builder.Property(batch => batch.Id).ValueGeneratedNever();
        builder.Property(batch => batch.Name).IsRequired().HasMaxLength(200);
        builder.Property(batch => batch.EncoderId);
        builder.Property(batch => batch.TransformationId).IsRequired();
        builder.Property(batch => batch.Status).HasConversion<string>().IsRequired().HasMaxLength(50);
        builder.Property(batch => batch.OriginalInputJson).IsRequired().HasColumnType("jsonb");
        builder.Property(batch => batch.NormalizedRowsJson).IsRequired().HasColumnType("jsonb");
        builder.Property(batch => batch.CreatedAt).IsRequired();
        builder.Property(batch => batch.UpdatedAt).IsRequired();
        builder.HasIndex(batch => batch.TransformationId);
        builder.HasIndex(batch => batch.EncoderId);
    }
}
