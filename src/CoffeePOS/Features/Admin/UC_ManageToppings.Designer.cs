using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageToppings
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Table _table = null!;
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
            PlaceholderText = "Tìm kiếm topping...",
            AllowClear = true,
            Font = UiTheme.BodyFont
        };

        _btnAdd = new AntdUI.Button
        {
            Text = "Thêm Topping",
            Type = TTypeMini.Primary,
            Font = UiTheme.BodyFont,
            Padding = new Padding(5, 0, 5, 0)
        };

        AntdUI.GridPanel trashLayout = new()
        {
            Span = "40% 60%",
        };

        AntdUI.Label lblTrash = new()
        {
            Text = "Thùng rác",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = UiTheme.BodyFont,
        };

        _switchTrash = new AntdUI.Switch();

        trashLayout.Controls.Add(lblTrash);
        trashLayout.Controls.Add(_switchTrash);

        AntdUI.GridPanel headerPanel = new()
        {
            Dock = DockStyle.Top,
            Height = 45,
            Span = "160 100% 160",
            Margin = new Padding(0, 0, 0, 10)
        };

        headerPanel.Controls.Add(trashLayout);
        headerPanel.Controls.Add(_txtSearch);
        headerPanel.Controls.Add(_btnAdd);

        _table = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Bordered = true,
            EmptyHeader = true,
            EmptyText = "Chưa có topping nào",
            AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill,
            Font = UiTheme.BodyFont
        };

        hostPanel.Controls.Add(_table);
        hostPanel.Controls.Add(headerPanel);
        Controls.Add(hostPanel);
    }
}
