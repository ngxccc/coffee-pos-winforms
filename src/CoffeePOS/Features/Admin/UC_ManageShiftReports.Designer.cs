
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

partial class UC_ManageShiftReports
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Table _tableShiftReports = null!;
    private AntdUI.Input _txtSearch = null!;

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
            PlaceholderText = "Tìm kiếm bản chốt ca...",
            AllowClear = true,
            Font = UiTheme.BodyFont
        };

        AntdUI.GridPanel trashLayout = new()
        {
            Span = "40% 60%",
        };

        AntdUI.GridPanel headerPanel = new()
        {
            Dock = DockStyle.Top,
            Height = 45,
            Span = "100%",
            Margin = new Padding(0, 0, 0, 10)
        };

        headerPanel.Controls.Add(trashLayout);
        headerPanel.Controls.Add(_txtSearch);

        _tableShiftReports = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Bordered = true,
            EmptyHeader = true,
            EmptyText = "Chưa có bản chốt ca nào",
            AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill,
            Font = UiTheme.BodyFont
        };

        hostPanel.Controls.Add(_tableShiftReports);
        hostPanel.Controls.Add(headerPanel);
        Controls.Add(hostPanel);
    }
}
