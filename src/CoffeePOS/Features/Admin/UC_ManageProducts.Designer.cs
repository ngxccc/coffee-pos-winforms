using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

partial class UC_ManageProducts
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Table _tableProducts = null!;
    private AntdUI.Input _txtSearch = null!;
    private AntdUI.Switch _switchTrash = null!;
    private AntdUI.Button _btnAdd = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        AntdUI.Panel hostPanel = new()
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Back = UiTheme.Surface,
            Padding = new Padding(UiTheme.BlockGap)
        };

        _txtSearch = new AntdUI.Input
        {
            PlaceholderText = "Tìm kiếm sản phẩm...",
            AllowClear = true,
            Font = UiTheme.BodyFont
        };

        _btnAdd = new AntdUI.Button
        {
            Text = "Thêm sản phẩm",
            Type = TTypeMini.Primary,
            Font = UiTheme.BodyFont,
            Padding = new Padding(5, 0, 5, 0)
        };

        AntdUI.GridPanel trashLayout = new()
        {
            Span = "40% 60%",
            Gap = 4
        };

        AntdUI.Label lblTrash = new()
        {
            Text = "Thùng rác",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = UiTheme.BodyFont
        };

        _switchTrash = new AntdUI.Switch();

        trashLayout.Controls.Add(lblTrash);
        trashLayout.Controls.Add(_switchTrash);

        AntdUI.GridPanel headerPanel = new()
        {
            Dock = DockStyle.Top,
            Height = 70,
            Span = "160 100% 200",
            Gap = 10,
            Margin = new Padding(0, 0, 0, 10)
        };

        headerPanel.Controls.Add(trashLayout);
        headerPanel.Controls.Add(_txtSearch);
        headerPanel.Controls.Add(_btnAdd);

        _tableProducts = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Bordered = true,
            EmptyHeader = true,
            AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill,
            Font = UiTheme.BodyFont
        };

        hostPanel.Controls.Add(_tableProducts);
        hostPanel.Controls.Add(headerPanel);
        Controls.Add(hostPanel);
    }
}
