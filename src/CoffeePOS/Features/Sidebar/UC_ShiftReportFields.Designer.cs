using System.Drawing;
using System.Windows.Forms;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Sidebar;

public partial class UC_ShiftReportFields
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Label _lblHeader = null!;
    private AntdUI.Label _lblTotalBills = null!;
    private AntdUI.Label _lblExpectedCash = null!;
    private AntdUI.InputNumber _numStartingCash = null!; // HACK: Ô nhập tiền vốn
    private AntdUI.InputNumber _numActualCash = null!;   // HACK: Nâng cấp thành InputNumber
    private AntdUI.Input _txtNote = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        Size = new Size(400, 470);

        AntdUI.Divider divTitle = new()
        {
            Text = "CHỐT CA LÀM VIỆC",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            ColorSplit = UiTheme.BrandPrimary,
            Height = 30,
            Margin = new Padding(0, 0, 0, 10)
        };

        AntdUI.StackPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
        };

        _lblHeader = new AntdUI.Label
        {
            Text = "Nhân viên: N/A",
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.BrandPrimary,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        _lblTotalBills = new AntdUI.Label
        {
            Text = "Tổng hoá đơn: Đang tải...",
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
        };

        _lblExpectedCash = new AntdUI.Label
        {
            Text = "Tiền trên hệ thống: [ĐÃ ẨN]",
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 15)
        };

        AntdUI.Label lblStartingTitle = new()
        {
            Text = "Tiền làm vốn (Tiền lẻ đầu ca):",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
        };

        _numStartingCash = new AntdUI.InputNumber
        {
            Font = UiTheme.BodyFont,
            Height = 40,
            Minimum = 0,
            Maximum = 1000000000,
            Value = 500000,
            ThousandsSeparator = true,
            Margin = new Padding(0, 0, 0, 10),
            Enabled = false
        };

        AntdUI.Label lblActualTitle = new()
        {
            Text = "Nhập số tiền mặt thực đếm trong két:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
        };

        _numActualCash = new AntdUI.InputNumber
        {
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Height = 45,
            Minimum = 0,
            Maximum = 1000000000,
            Value = 0,
            ThousandsSeparator = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        AntdUI.Label lblNoteTitle = new()
        {
            Text = "Ghi chú (Lý do lệch tiền nếu có):",
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.TextPrimary,
            AutoSize = true,
        };

        _txtNote = new AntdUI.Input
        {
            Multiline = true,
            Font = UiTheme.BodyFont,
            Height = 100,
            AllowClear = true
        };

        mainLayout.Controls.Add(_txtNote);
        mainLayout.Controls.Add(lblNoteTitle);
        mainLayout.Controls.Add(_numActualCash);
        mainLayout.Controls.Add(lblActualTitle);
        mainLayout.Controls.Add(_numStartingCash);
        mainLayout.Controls.Add(lblStartingTitle);
        mainLayout.Controls.Add(_lblExpectedCash);
        mainLayout.Controls.Add(_lblTotalBills);
        mainLayout.Controls.Add(_lblHeader);
        mainLayout.Controls.Add(divTitle);

        Controls.Add(mainLayout);
        ResumeLayout(false);
    }
}
