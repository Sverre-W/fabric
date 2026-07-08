using Fabric.Server.Desfire.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Desfire.Persistence.Configuration;

public sealed class DesfireVariableSequenceConfiguration : IEntityTypeConfiguration<DesfireVariableSequence>
{
    public void Configure(EntityTypeBuilder<DesfireVariableSequence> builder)
    {
        builder.ToTable("variable_sequences");
        builder.HasKey(sequence => sequence.Id);
        builder.Property(sequence => sequence.Id).ValueGeneratedNever();
        builder.Property(sequence => sequence.Name).IsRequired().HasMaxLength(200);
        builder.Property(sequence => sequence.NextValue).IsRequired();
        builder.Property(sequence => sequence.CreatedAt).IsRequired();
        builder.Property(sequence => sequence.UpdatedAt).IsRequired();
        builder.HasIndex(sequence => sequence.Name).IsUnique();
    }
}
