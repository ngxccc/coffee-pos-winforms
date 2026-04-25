using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

partial class UC_ManageProductSizes
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Table _tableSizes = null!;
    private AntdUI.Button _btnAdd = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        BackColor = UiTheme.Surface;
        AutoScaleMode = AutoScaleMode.Dpi;
        Size = LogicalToDeviceUnits(new Size(500, 400));

        AntdUI.Panel hostPanel = new()
        {
            Dock = DockStyle.Fill,
            Back = UiTheme.Surface,
        };

        _btnAdd = new AntdUI.Button
        {
            Text = "Thêm Size",
            Type = TTypeMini.Primary,
            Font = UiTheme.BodyFont,
            Padding = new Padding(5, 0, 5, 0)
        };

        AntdUI.GridPanel headerPanel = new()
        {
            Dock = DockStyle.Top,
            Height = 45,
            Span = "100% 120",
        };

        headerPanel.Controls.Add(_btnAdd);
        headerPanel.Controls.Add(new Control());

        _tableSizes = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Bordered = true,
            EmptyHeader = true,
            EmptyText = "Món này chưa được cấu hình Size",
            AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill,
            Font = UiTheme.BodyFont
        };

        hostPanel.Controls.Add(_tableSizes);
        hostPanel.Controls.Add(headerPanel);
        Controls.Add(hostPanel);
    }
}
