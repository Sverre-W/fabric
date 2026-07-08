using Fabric.Server.Desfire.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Desfire.Persistence.Configuration;

public sealed class ChipDesignConfiguration : IEntityTypeConfiguration<ChipDesign>
{
    public void Configure(EntityTypeBuilder<ChipDesign> builder)
    {
        builder.ToTable("chip_designs");
        builder.HasKey(design => design.Id);
        builder.Property(design => design.Id).ValueGeneratedNever();
        builder.Property(design => design.Name).IsRequired().HasMaxLength(200);
        builder.Property(design => design.Version).IsRequired();
        builder.Property(design => design.Description).HasMaxLength(1000);
        builder.Property(design => design.SpecificationJson).IsRequired().HasColumnType("jsonb");
        builder.Property(design => design.CreatedAt).IsRequired();
        builder.HasIndex(design => new { design.Name, design.Version }).IsUnique();
    }
}
