using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

partial class UC_ManageUsers
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Table _tableUsers = null!;
    private AntdUI.Input _txtSearch = null!;
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
            PlaceholderText = "Tìm kiếm theo tài khoản, họ tên...",
            AllowClear = true,
            Font = UiTheme.BodyFont
        };

        _btnAdd = new AntdUI.Button
        {
            Text = "Thêm nhân viên",
            Type = TTypeMini.Primary,
            Font = UiTheme.BodyFont,
            Padding = new Padding(5, 0, 5, 0)
        };

        AntdUI.GridPanel headerPanel = new()
        {
            Dock = DockStyle.Top,
            Height = 65,
            Span = "100% 160",
            Gap = 10,
            Margin = new Padding(0, 0, 0, 15)
        };

        headerPanel.Controls.Add(_btnAdd);
        headerPanel.Controls.Add(_txtSearch);

        _tableUsers = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Bordered = true,
            EmptyHeader = true,
            EmptyText = "Chưa có nhân viên nào",
            AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill,
            Font = UiTheme.BodyFont
        };

        hostPanel.Controls.Add(_tableUsers);
        hostPanel.Controls.Add(headerPanel);
        Controls.Add(hostPanel);
    }
}
