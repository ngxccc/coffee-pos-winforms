using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillHistory
{
    private System.ComponentModel.IContainer components = null!;
    private AntdUI.Table _tableBills = null!;
    private AntdUI.Label _lblTitle = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;
        Padding = new Padding(20);

        _lblTitle = new AntdUI.Label
        {
            Text = "LỊCH SỬ HOÁ ĐƠN TRONG NGÀY",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _tableBills = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            EmptyText = "Hôm nay chưa có đơn hàng nào",
            Radius = 8,
            BorderWidth = 1F,
            BorderColor = UiTheme.SurfaceAlt,
            Font = new Font("Segoe UI", 10),
        };

        Controls.Add(_tableBills);
        Controls.Add(_lblTitle);
    }
}
