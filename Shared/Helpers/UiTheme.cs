namespace CoffeePOS.Shared.Helpers;

public static class UiTheme
{
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.WhiteSmoke;
    public static readonly Color BrandPrimary = Color.FromArgb(0, 122, 204);
    public static readonly Color TextPrimary = Color.FromArgb(31, 30, 68);
    public static readonly Color SidebarHover = Color.FromArgb(41, 40, 78);
    public static readonly Color SidebarText = Color.Gainsboro;
    public static readonly Color SidebarTextActive = Color.White;

    public const int PagePadding = 14;
    public const int BlockGap = 10;
    public const int ToolbarHeight = 88;

    public static AntdUI.TTypeMini AddButtonType => AntdUI.TTypeMini.Success;
    public static AntdUI.TTypeMini EditButtonType => AntdUI.TTypeMini.Warn;
    public static AntdUI.TTypeMini DeleteButtonType => AntdUI.TTypeMini.Error;
    public static AntdUI.TTypeMini PrimaryButtonType => AntdUI.TTypeMini.Primary;
}
