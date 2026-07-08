using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Kiosk.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabric.Server.Kiosk.Persistence.Configuration;

public sealed class KioskProfileLanguageConfiguration : IEntityTypeConfiguration<KioskProfileLanguage>
{
    public void Configure(EntityTypeBuilder<KioskProfileLanguage> builder)
    {
        builder.ToTable("profile_languages");
        builder.HasKey(language => language.Id).HasName("pk_kiosk_profile_languages");
        builder.Property(language => language.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(language => language.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(language => language.LanguageCode).HasColumnName("language_code").IsRequired().HasMaxLength(20);
        builder.Property(language => language.DisplayName).HasColumnName("display_name").IsRequired().HasMaxLength(100);
        builder.Property(language => language.IsDefault).HasColumnName("is_default").IsRequired();
        builder.Property(language => language.SortOrder).HasColumnName("sort_order").IsRequired();
        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(KioskProfileLanguage.ProfileId), nameof(KioskProfileLanguage.LanguageCode)).HasDatabaseName("ix_kiosk_profile_languages_tenant_id_profile_id_language_code").IsUnique();
    }
}

public sealed class KioskTranslationConfiguration : IEntityTypeConfiguration<KioskTranslation>
{
    public void Configure(EntityTypeBuilder<KioskTranslation> builder)
    {
        builder.ToTable("translations");
        builder.HasKey(translation => translation.Id).HasName("pk_kiosk_translations");
        builder.Property(translation => translation.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(translation => translation.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(translation => translation.LanguageCode).HasColumnName("language_code").IsRequired().HasMaxLength(20);
        builder.Property(translation => translation.Key).HasColumnName("key").IsRequired().HasMaxLength(300);
        builder.Property(translation => translation.Value).HasColumnName("value").IsRequired();
        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(KioskTranslation.ProfileId), nameof(KioskTranslation.LanguageCode), nameof(KioskTranslation.Key)).HasDatabaseName("ix_kiosk_translations_tenant_profile_language_key").IsUnique();
    }
}

public sealed class KioskThemeTokenConfiguration : IEntityTypeConfiguration<KioskThemeToken>
{
    public void Configure(EntityTypeBuilder<KioskThemeToken> builder)
    {
        builder.ToTable("theme_tokens");
        builder.HasKey(token => token.Id).HasName("pk_kiosk_theme_tokens");
        builder.Property(token => token.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(token => token.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(token => token.Key).HasColumnName("key").IsRequired().HasMaxLength(100);
        builder.Property(token => token.Value).HasColumnName("value").IsRequired().HasMaxLength(500);
        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(KioskThemeToken.ProfileId), nameof(KioskThemeToken.Key)).HasDatabaseName("ix_kiosk_theme_tokens_tenant_profile_key").IsUnique();
    }
}

public sealed class KioskWelcomeSettingsConfiguration : IEntityTypeConfiguration<KioskWelcomeSettings>
{
    public void Configure(EntityTypeBuilder<KioskWelcomeSettings> builder)
    {
        builder.ToTable("welcome_settings");
        builder.HasKey(settings => settings.ProfileId).HasName("pk_kiosk_welcome_settings");
        builder.Property(settings => settings.ProfileId).HasColumnName("profile_id").ValueGeneratedNever();
        builder.Property(settings => settings.TitleKey).HasColumnName("title_key").IsRequired().HasMaxLength(300);
        builder.Property(settings => settings.SubtitleKey).HasColumnName("subtitle_key").HasMaxLength(300);
        builder.Property(settings => settings.StartButtonKey).HasColumnName("start_button_key").IsRequired().HasMaxLength(300);
        builder.Property(settings => settings.BackgroundAssetName).HasColumnName("background_asset_name").HasMaxLength(200);
        builder.Property(settings => settings.LogoAssetName).HasColumnName("logo_asset_name").HasMaxLength(200);
        TenantDbContext.ConfigureTenantProperty(builder);
    }
}

public sealed class KioskHardwareBindingConfiguration : IEntityTypeConfiguration<KioskHardwareBinding>
{
    public void Configure(EntityTypeBuilder<KioskHardwareBinding> builder)
    {
        builder.ToTable("hardware_bindings");
        builder.HasKey(binding => binding.Id).HasName("pk_kiosk_hardware_bindings");
        builder.Property(binding => binding.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(binding => binding.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(binding => binding.BindingKey).HasColumnName("binding_key").IsRequired().HasMaxLength(100);
        builder.Property(binding => binding.DisplayName).HasColumnName("display_name").IsRequired().HasMaxLength(200);
        builder.Property(binding => binding.RequiredCapability).HasColumnName("required_capability").IsRequired().HasMaxLength(100);
        builder.Property(binding => binding.Required).HasColumnName("required").IsRequired();
        builder.Property(binding => binding.CleanupOnSessionEnd).HasColumnName("cleanup_on_session_end").IsRequired();
        builder.Property(binding => binding.SortOrder).HasColumnName("sort_order").IsRequired();
        TenantDbContext.ConfigureTenantProperty(builder);
        builder.HasIndex(TenantDbContext.TenantIdPropertyName, nameof(KioskHardwareBinding.ProfileId), nameof(KioskHardwareBinding.BindingKey)).HasDatabaseName("ix_kiosk_hardware_bindings_tenant_profile_key").IsUnique();
    }
}
