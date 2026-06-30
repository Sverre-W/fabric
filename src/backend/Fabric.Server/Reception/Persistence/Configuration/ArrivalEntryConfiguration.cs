using Fabric.Server.Reception.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Reception.Persistence.Configuration;

public sealed class ArrivalEntryConfiguration : IEntityTypeConfiguration<ArrivalEntry>
{
    public void Configure(EntityTypeBuilder<ArrivalEntry> builder)
    {
        builder.ToTable("arrival_entries");

        builder.HasKey(e => e.Id).HasName("pk_arrival_entries");

        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("expected_arrival_id").HasColumnName("expected_arrival_id").IsRequired();
        builder.Property(e => e.Type).HasColumnName("type").IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Timestamp).HasColumnName("timestamp").IsRequired();
        ConfigureActor(builder.OwnsOne(e => e.Actor), "actor");

        builder.HasIndex("expected_arrival_id").HasDatabaseName("ix_arrival_entries_expected_arrival_id");
    }

    private static void ConfigureActor(OwnedNavigationBuilder<ArrivalEntry, ReceptionActor> builder, string prefix)
    {
        builder.Property(actor => actor.Type).HasColumnName($"{prefix}_type").HasConversion<string>().HasMaxLength(20);
        builder.Property(actor => actor.Identifier).HasColumnName($"{prefix}_identifier").HasMaxLength(320);
        builder.Property(actor => actor.DisplayName).HasColumnName($"{prefix}_display_name").HasMaxLength(200);
    }
}
