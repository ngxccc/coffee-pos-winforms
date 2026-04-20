using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillDetail
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Label _lblTitle = null!;
    private AntdUI.GridPanel _pnlSummary = null!;
    private AntdUI.Label _lblBuzzer = null!;
    private AntdUI.Label _lblStaff = null!;
    private AntdUI.Label _lblCreatedAt = null!;
    private AntdUI.Label _lblStatus = null!;
    private AntdUI.Label _lblCanceledAt = null!;

    private AntdUI.Table _tableItems = null!;
    private AntdUI.Label _lblTotal = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Size = LogicalToDeviceUnits(new Size(700, 550));
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        SuspendLayout();

        _lblTitle = new AntdUI.Label
        {
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Height = 45,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 0, 0, 10)
        };

        _pnlSummary = new AntdUI.GridPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Span = "50% 50%; 50% 50%",
            Gap = 2,
            Margin = new Padding(0, 0, 0, 15)
        };
        _pnlSummary.SuspendLayout();

        Font summaryFont = new Font("Segoe UI", 11);
        Color summaryColor = UiTheme.BrandPrimary;

        _lblBuzzer = new AntdUI.Label { Font = summaryFont, ForeColor = summaryColor };
        _lblStaff = new AntdUI.Label { Font = summaryFont, ForeColor = summaryColor };
        _lblCreatedAt = new AntdUI.Label { Font = summaryFont, ForeColor = summaryColor };
        _lblStatus = new AntdUI.Label { Font = summaryFont, ForeColor = summaryColor };
        _lblCanceledAt = new AntdUI.Label { Font = summaryFont, ForeColor = summaryColor };

        _pnlSummary.Controls.Add(_lblCreatedAt);
        _pnlSummary.Controls.Add(_lblBuzzer);
        _pnlSummary.Controls.Add(_lblStatus);
        _pnlSummary.Controls.Add(_lblStaff);

        _lblTotal = new AntdUI.Label
        {
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Dock = DockStyle.Bottom,
            Height = 50,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 20, 0)
        };

        _tableItems = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            BackColor = UiTheme.Surface,
            BorderWidth = 1F,
            BorderColor = UiTheme.SurfaceAlt,
            EmptyText = "Không có dữ liệu",
            Font = new Font("Segoe UI", 10),
            AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill,
        };

        Controls.Add(_tableItems);
        Controls.Add(_lblTotal);
        Controls.Add(_pnlSummary);
        Controls.Add(_lblTitle);

        _pnlSummary.ResumeLayout(false);
        ResumeLayout(false);
    }
}
