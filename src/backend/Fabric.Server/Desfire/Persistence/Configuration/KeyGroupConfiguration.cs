using Fabric.Server.Desfire.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Desfire.Persistence.Configuration;

public sealed class KeyGroupConfiguration : IEntityTypeConfiguration<KeyGroup>
{
    public void Configure(EntityTypeBuilder<KeyGroup> builder)
    {
        builder.ToTable("key_groups");
        builder.HasKey(group => group.Id);
        builder.Property(group => group.Id).ValueGeneratedNever();
        builder.Property(group => group.Name).IsRequired().HasMaxLength(200);
        builder.Property(group => group.KeyType).HasConversion<string>().IsRequired().HasMaxLength(80);
        builder.Property(group => group.Locked).IsRequired();
        builder.Property(group => group.DiversificationStrategyId);
        builder.Property(group => group.CreatedAt).IsRequired();
        builder.Property(group => group.UpdatedAt).IsRequired();
        builder.HasIndex(group => group.Name).IsUnique();

        builder.OwnsMany(group => group.KeySets, keySets =>
        {
            keySets.ToTable("key_group_key_sets");
            keySets.WithOwner().HasForeignKey("key_group_id");
            keySets.HasKey(keySet => keySet.Id);
            keySets.Property(keySet => keySet.Id).ValueGeneratedNever();
            keySets.Property(keySet => keySet.KeySetId).IsRequired();
            keySets.HasIndex("key_group_id", nameof(KeyGroupKeySet.KeySetId)).IsUnique();

            keySets.OwnsMany(keySet => keySet.Keys, keys =>
            {
                keys.ToTable("key_group_keys");
                keys.WithOwner().HasForeignKey("key_set_id");
                keys.HasKey(key => key.Id);
                keys.Property(key => key.Id).ValueGeneratedNever();
                keys.Property(key => key.KeyId).IsRequired();
                keys.Property(key => key.ProtectedValue).IsRequired();
                keys.Property(key => key.IsDiversified).IsRequired();
                keys.HasIndex("key_set_id", nameof(KeyGroupKey.KeyId)).IsUnique();
            });
        });
    }
}
