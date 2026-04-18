using Microsoft.Extensions.Configuration;

namespace CoffeePOS.Shared.Helpers;

public static class UiTheme
{
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
        Surface = ParseColor(config["ThemeSettings:Surface"], Surface);
        SurfaceAlt = ParseColor(config["ThemeSettings:SurfaceAlt"], SurfaceAlt);
        BrandPrimary = ParseColor(config["ThemeSettings:BrandPrimary"], BrandPrimary);
        TextPrimary = ParseColor(config["ThemeSettings:TextPrimary"], TextPrimary);
        SidebarHover = ParseColor(config["ThemeSettings:SidebarHover"], SidebarHover);
        SidebarText = ParseColor(config["ThemeSettings:SidebarText"], SidebarText);
        SidebarTextActive = ParseColor(config["ThemeSettings:SidebarTextActive"], SidebarTextActive);
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
            return ColorTranslator.FromHtml(hex);
        }
        catch
        {
            return fallback;
        }
    }
}
