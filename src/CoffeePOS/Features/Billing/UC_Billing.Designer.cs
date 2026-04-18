using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_Billing
{
    private System.ComponentModel.IContainer components = null!;

    private FlowLayoutPanel _flowBillItemList = null!;
    private AntdUI.Label _lblTotalPrice = null!;
    private AntdUI.Label _lblOrderTitle = null!;
    private AntdUI.Button _btnPay = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Width = 420;
        Dock = DockStyle.Right;
        BackColor = UiTheme.Surface;

        SuspendLayout();

        AntdUI.Panel pnlFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 130,
            Padding = new Padding(20),
            Back = UiTheme.SurfaceAlt,
            Radius = 0
        };

        AntdUI.Panel pnlTotalInfo = new()
        {
            Dock = DockStyle.Top,
            Height = 35,
            Back = UiTheme.SurfaceAlt
        };

        AntdUI.Label lblTotalTitle = new()
        {
            Text = "Tổng cộng:",
            Dock = DockStyle.Left,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            BackColor = UiTheme.SurfaceAlt,
            Width = 120,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _lblTotalPrice = new()
        {
            Text = "0 đ",
            Dock = DockStyle.Right,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            BackColor = UiTheme.SurfaceAlt,
            Width = 200,
            TextAlign = ContentAlignment.MiddleRight
        };

        pnlTotalInfo.Controls.Add(_lblTotalPrice);
        pnlTotalInfo.Controls.Add(lblTotalTitle);

        _btnPay = new()
        {
            Text = "THANH TOÁN",
            Dock = DockStyle.Bottom,
            Height = 50,
            Type = TTypeMini.Primary,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Radius = 8,
            BorderWidth = 0,
            Margin = new Padding(0, 10, 0, 0)
        };

        pnlFooter.Controls.Add(_btnPay);
        pnlFooter.Controls.Add(pnlTotalInfo);

        _flowBillItemList = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10),
            BackColor = UiTheme.Surface
        };

        AntdUI.Divider divider1 = new()
        {
            Dock = DockStyle.Left,
            Thickness = 1F,
            Size = new Size(1, 1000),
            Margin = new Padding(0),
            Vertical = true
        };

        Controls.Add(_flowBillItemList);
        Controls.Add(divider1);
        Controls.Add(pnlFooter);

        ResumeLayout(false);
    }
}
