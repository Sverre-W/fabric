namespace Fabric.Server.Kiosk.Domain;

public sealed class KioskProfile
{
    private KioskProfile() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string DefaultLanguageCode { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static KioskProfile Create(string name, string defaultLanguageCode, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        DefaultLanguageCode = NormalizeLanguage(defaultLanguageCode),
        CreatedAt = now,
        UpdatedAt = now
    };

    public void Update(string name, string defaultLanguageCode, DateTimeOffset now)
    {
        Name = name.Trim();
        DefaultLanguageCode = NormalizeLanguage(defaultLanguageCode);
        UpdatedAt = now;
    }

    public static string NormalizeLanguage(string languageCode) => languageCode.Trim();
}
