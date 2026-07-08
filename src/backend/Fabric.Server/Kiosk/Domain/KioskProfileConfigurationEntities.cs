namespace Fabric.Server.Kiosk.Domain;

public sealed class KioskProfileLanguage
{
    private KioskProfileLanguage() { }

    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public string LanguageCode { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public bool IsDefault { get; private set; }
    public int SortOrder { get; private set; }

    public static KioskProfileLanguage Create(Guid profileId, string languageCode, string displayName, bool isDefault, int sortOrder) => new()
    {
        Id = Guid.NewGuid(),
        ProfileId = profileId,
        LanguageCode = KioskProfile.NormalizeLanguage(languageCode),
        DisplayName = displayName.Trim(),
        IsDefault = isDefault,
        SortOrder = sortOrder
    };
}

public sealed class KioskTranslation
{
    private KioskTranslation() { }

    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public string LanguageCode { get; private set; } = default!;
    public string Key { get; private set; } = default!;
    public string Value { get; private set; } = default!;

    public static KioskTranslation Create(Guid profileId, string languageCode, string key, string value) => new()
    {
        Id = Guid.NewGuid(),
        ProfileId = profileId,
        LanguageCode = KioskProfile.NormalizeLanguage(languageCode),
        Key = key.Trim(),
        Value = value
    };
}

public sealed class KioskThemeToken
{
    private KioskThemeToken() { }

    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public string Key { get; private set; } = default!;
    public string Value { get; private set; } = default!;

    public static KioskThemeToken Create(Guid profileId, string key, string value) => new()
    {
        Id = Guid.NewGuid(),
        ProfileId = profileId,
        Key = key.Trim(),
        Value = value.Trim()
    };
}

public sealed class KioskWelcomeSettings
{
    private KioskWelcomeSettings() { }

    public Guid ProfileId { get; private set; }
    public string TitleKey { get; private set; } = default!;
    public string? SubtitleKey { get; private set; }
    public string StartButtonKey { get; private set; } = default!;
    public string? BackgroundAssetName { get; private set; }
    public string? LogoAssetName { get; private set; }

    public static KioskWelcomeSettings Create(Guid profileId, string titleKey, string? subtitleKey, string startButtonKey, string? backgroundAssetName, string? logoAssetName) => new()
    {
        ProfileId = profileId,
        TitleKey = titleKey.Trim(),
        SubtitleKey = string.IsNullOrWhiteSpace(subtitleKey) ? null : subtitleKey.Trim(),
        StartButtonKey = startButtonKey.Trim(),
        BackgroundAssetName = string.IsNullOrWhiteSpace(backgroundAssetName) ? null : backgroundAssetName.Trim(),
        LogoAssetName = string.IsNullOrWhiteSpace(logoAssetName) ? null : logoAssetName.Trim()
    };
}

public sealed class KioskHardwareBinding
{
    private KioskHardwareBinding() { }

    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public string BindingKey { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string RequiredCapability { get; private set; } = default!;
    public bool Required { get; private set; }
    public bool CleanupOnSessionEnd { get; private set; }
    public int SortOrder { get; private set; }

    public static KioskHardwareBinding Create(Guid profileId, string bindingKey, string displayName, string requiredCapability, bool required, bool cleanupOnSessionEnd, int sortOrder) => new()
    {
        Id = Guid.NewGuid(),
        ProfileId = profileId,
        BindingKey = NormalizeBindingKey(bindingKey),
        DisplayName = displayName.Trim(),
        RequiredCapability = requiredCapability.Trim(),
        Required = required,
        CleanupOnSessionEnd = cleanupOnSessionEnd,
        SortOrder = sortOrder
    };

    public static string NormalizeBindingKey(string bindingKey) => bindingKey.Trim().ToLowerInvariant();
}
