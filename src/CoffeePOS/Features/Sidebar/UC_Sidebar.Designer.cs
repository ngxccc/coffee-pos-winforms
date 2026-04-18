using AntdUI;
using CoffeePOS.Shared.Constants;
namespace CoffeePOS.Features.Sidebar;

partial class UC_Sidebar
{
    private System.ComponentModel.IContainer components = null!;
    private AntdUI.TooltipComponent _tooltipToggle = null!;
    private AntdUI.Menu _menuMain = null!;
    private AntdUI.Panel _panelFooter = null!;
    private AntdUI.Button _btnToggle = null!;
    // TODO: Dynamic width via _menuMain collapsed
    private int _sidebarWidth = 200;
    private int _sidebarWidthCollaped = 80;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        Margin = new Padding(0);
        Name = "UC_Sidebar";
        Size = new Size(_sidebarWidthCollaped, 800);
        Dock = DockStyle.Left;
        SuspendLayout();

        _panelFooter = new()
        {
            Dock = DockStyle.Bottom,
            Name = "panelFooter",
            Size = new Size(_sidebarWidth, 48),
            Margin = new Padding(5)
        };
        _panelFooter.SuspendLayout();

        AntdUI.Divider divider1 = new()
        {
            Dock = DockStyle.Right,
            Size = new Size(1, 1000),
            Thickness = 1F,
            Vertical = true,
            Margin = new Padding(0)
        };

        _btnToggle = new()
        {
            Dock = DockStyle.Fill,
            Name = "btnToggle",
            TabIndex = 2,
            IconSvg = SvgAssets.MenuFoldOutlined,
            Ghost = true,
            Radius = 10,
            IconSize = new Size(20, 20)
        };

        _tooltipToggle = new();
        // HACK: Add to GC container properly since it lacks an (IContainer) constructor
        components.Add(_tooltipToggle);
        _tooltipToggle.SetTip(_btnToggle, "Thu gọn / mở rộng sidebar");

        _menuMain = new()
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F),
            Name = "menuMain",
            TabIndex = 1,
            Collapsed = true,
            Padding = new Padding(5)
        };

        _panelFooter.Controls.Add(_btnToggle);

        // NOTE: Z-order is extremely important for Fill/Dock calculations
        Controls.Add(_menuMain);
        Controls.Add(_panelFooter);
        Controls.Add(divider1);

        _panelFooter.ResumeLayout(false);
        ResumeLayout(false);
    }
}
