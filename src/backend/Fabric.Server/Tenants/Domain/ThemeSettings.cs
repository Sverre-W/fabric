namespace Fabric.Server.Tenants.Domain;

public sealed record ThemeSettings
{
    public static ThemeSettings Default { get; } = new();

    public string BackgroundColor { get; init; } = "#f8f8f8";
    public string ContentColor { get; init; } = "#ffffff";
    public string PrimaryColor { get; init; } = "#238cff";
    public string TextColor { get; init; } = "#212529";
    public string TextMutedColor { get; init; } = "#6c757d";
    public string BorderColor { get; init; } = "#dddddd";
    public string HoverBlueColor { get; init; } = "#eef6ff";
    public string ActiveBlueColor { get; init; } = "#deeeff";
    public string HoverGrayColor { get; init; } = "#f3f3f3";
    public string ErrorColor { get; init; } = "#ff6467";
    public string ErrorBackgroundColor { get; init; } = "#feeaea";
    public string DangerColor { get; init; } = "#ff6467";
    public string SuccessColor { get; init; } = "#00c950";
    public string SuccessBackgroundColor { get; init; } = "#e6faeb";
}
