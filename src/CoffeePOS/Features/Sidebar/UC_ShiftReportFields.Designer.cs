using System.Drawing;
using System.Windows.Forms;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Sidebar;

public partial class UC_ShiftReportFields
{
    // WHY: Required by WinForms designer to manage component lifecycles
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Label _lblHeader = null!;
    private AntdUI.Label _lblTotalBills = null!;
    private AntdUI.Label _lblExpectedCash = null!;
    private AntdUI.Input _txtActualCash = null!;
    private AntdUI.Input _txtNote = null!;

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
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        AntdUI.Divider divTitle = new()
        {
            Text = "BÁO CÁO CA LÀM VIỆC",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            ColorSplit = UiTheme.BrandPrimary,
            Dock = DockStyle.Fill,
            Height = 40,
            Margin = new Padding(0, 10, 0, 10)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(20, 10, 20, 20)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        _lblHeader = new AntdUI.Label
        {
            Text = "Nhân viên: N/A",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        _lblTotalBills = new AntdUI.Label
        {
            Text = "Tổng hoá đơn: Đang tải...",
            Font = new Font("Segoe UI", 11),
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        _lblExpectedCash = new AntdUI.Label
        {
            Text = "Tiền trên hệ thống: [ĐÃ ẨN]",
            Font = new Font("Segoe UI", 11),
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        var lblInputTitle = new AntdUI.Label
        {
            Text = "Nhập số tiền mặt thực tế trong két:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };

        _txtActualCash = new AntdUI.Input
        {
            Font = new Font("Segoe UI", 13),
            Margin = new Padding(0, 0, 0, 10),
            PlaceholderText = "VD: 2500000",
            Dock = DockStyle.Fill,
            Height = 40
        };

        var lblNote = new AntdUI.Label
        {
            Text = "Ghi chú (lý do lệch tiền nếu có):",
            Font = new Font("Segoe UI", 10),
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };

        _txtNote = new AntdUI.Input
        {
            Multiline = true,
            Font = new Font("Segoe UI", 10),
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };

        layout.Controls.Add(divTitle, 0, 0);
        layout.Controls.Add(_lblHeader, 0, 1);
        layout.Controls.Add(_lblTotalBills, 0, 2);
        layout.Controls.Add(_lblExpectedCash, 0, 3);
        layout.Controls.Add(lblInputTitle, 0, 4);
        layout.Controls.Add(_txtActualCash, 0, 5);
        layout.Controls.Add(lblNote, 0, 6);
        layout.Controls.Add(_txtNote, 0, 7);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Controls.Add(layout);
    }
}
