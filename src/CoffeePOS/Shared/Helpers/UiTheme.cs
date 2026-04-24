using Microsoft.Extensions.Configuration;

namespace CoffeePOS.Shared.Helpers;

public static class UiTheme
{
    public const string PrimaryFontFamily = "Segoe UI";
    public static readonly Font TitleFont = new(PrimaryFontFamily, 16F, FontStyle.Bold);
    public static readonly Font HeaderFont = new(PrimaryFontFamily, 12F, FontStyle.Bold);
    public static readonly Font BodyFont = new(PrimaryFontFamily, 11F, FontStyle.Regular);
    public static readonly Font SmallFont = new(PrimaryFontFamily, 9F, FontStyle.Regular);

    public static Color Surface { get; private set; } = Color.White;
    public static Color SurfaceAlt { get; private set; } = Color.WhiteSmoke;
    public static Color BrandPrimary { get; private set; } = Color.FromArgb(0, 122, 204);
    public static Color TextPrimary { get; private set; } = Color.FromArgb(31, 30, 68);
    public static Color SidebarHover { get; private set; } = Color.FromArgb(41, 40, 78);
    public static Color SidebarText { get; private set; } = Color.Gainsboro;
    public static Color SidebarTextActive { get; private set; } = Color.White;

    public const int PagePadding = 14;
    public const int BlockGap = 10;
    public const int ToolbarHeight = 88;

    public static AntdUI.TTypeMini AddButtonType => AntdUI.TTypeMini.Success;
    public static AntdUI.TTypeMini EditButtonType => AntdUI.TTypeMini.Warn;
    public static AntdUI.TTypeMini DeleteButtonType => AntdUI.TTypeMini.Error;
    public static AntdUI.TTypeMini PrimaryButtonType => AntdUI.TTypeMini.Primary;

    // PERF: Strict single-pass initialization at Startup blocks continuous overhead during UI repaints.
    public static void LoadFromConfig(IConfiguration config)
    {
        var themeConfig = config.GetSection("ThemeSettings");
        if (!themeConfig.Exists()) return;

        Surface = ParseColor(themeConfig["Surface"], Surface);
        SurfaceAlt = ParseColor(themeConfig["SurfaceAlt"], SurfaceAlt);
        BrandPrimary = ParseColor(themeConfig["BrandPrimary"], BrandPrimary);
        TextPrimary = ParseColor(themeConfig["TextPrimary"], TextPrimary);
        SidebarHover = ParseColor(themeConfig["SidebarHover"], SidebarHover);
        SidebarText = ParseColor(themeConfig["SidebarText"], SidebarText);
        SidebarTextActive = ParseColor(themeConfig["SidebarTextActive"], SidebarTextActive);
    }

    public static void ApplyTheme()
    {
        AntdUI.Config.TextRenderingHighQuality = true;
        AntdUI.Config.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        AntdUI.Style.SetPrimary(BrandPrimary);
        AntdUI.Config.Theme()
            .Dark("#000", "#fff")
            .Light(ColorTranslator.ToHtml(Surface), ColorTranslator.ToHtml(TextPrimary))
            .FormBorderColor();
    }

    // WHY: Defensive programming. Silently swallows typo crashes in config files and falls back to default palette.
    private static Color ParseColor(string? hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return fallback;
        try
        {
            return ColorTranslator.FromHtml(hex.Trim());
        }
        catch
        {
            return fallback;
        }
    }
}
