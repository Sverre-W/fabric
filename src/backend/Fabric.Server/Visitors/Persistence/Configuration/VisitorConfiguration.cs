using Fabric.Server.Visitors.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Visitors.Persistence.Configuration;

public sealed class VisitorConfiguration : IEntityTypeConfiguration<Visitor>
{
    public void Configure(EntityTypeBuilder<Visitor> builder)
    {
        builder.ToTable("visitors");

        builder.HasKey(visitor => visitor.Id).HasName("pk_visitors");

        builder.Property(visitor => visitor.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(visitor => visitor.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(200);
        builder.Property(visitor => visitor.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(200);
        builder.Property(visitor => visitor.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
        builder.Property(visitor => visitor.Company).HasColumnName("company").IsRequired().HasMaxLength(200);

        builder.HasIndex(visitor => visitor.Email).IsUnique().HasDatabaseName("ix_visitors_email");
    }
}
