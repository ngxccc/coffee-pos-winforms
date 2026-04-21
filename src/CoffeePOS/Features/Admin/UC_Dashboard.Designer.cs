using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace CoffeePOS.Features.Admin;

public partial class UC_Dashboard
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Label _lblTodayRevenue = null!;
    private AntdUI.Label _lblTodayOrders = null!;
    private AntdUI.Label _lblTodayAverageOrder = null!;

    private AntdUI.Chart _chartRevenue = null!;
    private AntdUI.Chart _chartTopProducts = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        var mainGrid = new GridPanel
        {
            Dock = DockStyle.Fill,
            Span = "140: 100%; 100%",
            Gap = 2,
        };

        var kpiGrid = new GridPanel
        {
            Span = "33.3% 33.3% 33.3%",
            Gap = 20,
            Size = new Size(40, 20)
        };

        kpiGrid.Controls.Add(CreateKpiCard("DOANH THU HÔM NAY", out _lblTodayRevenue));
        kpiGrid.Controls.Add(CreateKpiCard("SỐ ĐƠN HÀNG", out _lblTodayOrders));
        kpiGrid.Controls.Add(CreateKpiCard("GIÁ TRỊ TRUNG BÌNH", out _lblTodayAverageOrder));

        var chartGrid = new GridPanel
        {
            Span = "65% 35%",
            Gap = 20,
            Dock = DockStyle.Fill
        };

        _chartRevenue = new AntdUI.Chart
        {
            ChartType = TChartType.Bar,
            ShowGrid = true,
            ShowAxes = true,
            ShowTooltip = true,
            EnableAnimation = true,
            Dock = DockStyle.Fill,
        };

        _chartTopProducts = new AntdUI.Chart
        {
            Dock = DockStyle.Fill,
            ChartType = TChartType.Doughnut,
            ShowGrid = false,
            ShowAxes = false,
            ShowTooltip = true,
            EnableAnimation = true,
            PieColors = new Color[]
            {
                Color.FromArgb(35, 137, 255),
                Color.FromArgb(13, 204, 204),
                Color.FromArgb(241, 142, 86),
                Color.FromArgb(215, 135, 255),
                Color.FromArgb(104, 199, 56)
            }
        };

        chartGrid.Controls.Add(CreateChartCard("Top 5 sản phẩm bán chạy", _chartTopProducts));
        chartGrid.Controls.Add(CreateChartCard("Xu hướng doanh thu 7 ngày", _chartRevenue));

        mainGrid.Controls.Add(chartGrid);
        mainGrid.Controls.Add(kpiGrid);

        Controls.Add(mainGrid);
    }

    private AntdUI.Panel CreateKpiCard(string title, out AntdUI.Label lblValue)
    {
        AntdUI.Panel card = new()
        {
            Back = UiTheme.SurfaceAlt,
            Radius = 12,
            Padding = new Padding(10)
        };

        AntdUI.Label lblTitle = new()
        {
            Text = title,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.Gray,
            BackColor = UiTheme.SurfaceAlt,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 20,
        };

        lblValue = new()
        {
            Text = "Đang tải...",
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            BackColor = UiTheme.SurfaceAlt,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
        };

        card.Controls.Add(lblValue);
        card.Controls.Add(lblTitle);
        return card;
    }

    private AntdUI.Panel CreateChartCard(string title, Control chartControl)
    {
        AntdUI.Panel card = new()
        {
            Back = UiTheme.SurfaceAlt,
            Radius = 12,
            Padding = new Padding(10),
            Dock = DockStyle.Fill
        };

        AntdUI.Label lblTitle = new()
        {
            Text = title,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            BackColor = UiTheme.SurfaceAlt,
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.TopCenter
        };

        card.Controls.Add(chartControl);
        card.Controls.Add(lblTitle);
        return card;
    }
}
