using Fabric.Server.Desfire.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Desfire.Persistence.Configuration;

public sealed class TransformationConfiguration : IEntityTypeConfiguration<Transformation>
{
    public void Configure(EntityTypeBuilder<Transformation> builder)
    {
        builder.ToTable("transformations");
        builder.HasKey(transformation => transformation.Id);
        builder.Property(transformation => transformation.Id).ValueGeneratedNever();
        builder.Property(transformation => transformation.Name).IsRequired().HasMaxLength(200);
        builder.Property(transformation => transformation.FromChipDesignName).HasMaxLength(200);
        builder.Property(transformation => transformation.FromBlank).IsRequired();
        builder.Property(transformation => transformation.ToChipDesignName).IsRequired().HasMaxLength(200);
        builder.Property(transformation => transformation.AlwaysReadUid).IsRequired();
        builder.Property(transformation => transformation.RequiredVariablesJson).IsRequired().HasColumnType("jsonb");
        builder.Property(transformation => transformation.RequiredKeyGroupsJson).IsRequired().HasColumnType("jsonb");
        builder.Property(transformation => transformation.VariableConfigsJson).IsRequired().HasColumnType("jsonb");
        builder.Property(transformation => transformation.CreatedAt).IsRequired();
        builder.Property(transformation => transformation.UpdatedAt).IsRequired();
        builder.HasIndex(transformation => transformation.Name).IsUnique();
    }
}
