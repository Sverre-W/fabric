using Fabric.Server.Reception.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Reception.Persistence.Configuration;

public sealed class CheckInDocumentConfiguration : IEntityTypeConfiguration<CheckInDocument>
{
    public void Configure(EntityTypeBuilder<CheckInDocument> builder)
    {
        builder.ToTable("check_in_documents");

        builder.HasKey(d => d.Id).HasName("pk_check_in_documents");

        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("expected_arrival_id").HasColumnName("expected_arrival_id").IsRequired();
        builder.Property(d => d.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(d => d.DocumentType).HasColumnName("document_type").IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.Content).HasColumnName("content").IsRequired();

        builder.HasIndex("expected_arrival_id").HasDatabaseName("ix_check_in_documents_expected_arrival_id");
    }
}