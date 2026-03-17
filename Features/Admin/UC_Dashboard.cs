using System.Globalization;
using CoffeePOS.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace CoffeePOS.Features.Admin;

public class UC_Dashboard : UserControl
{
    private readonly IDashboardService _dashboardService;

    // UI Controls
    private Label lblTodayRevenue = null!;
    private Label lblTodayOrders = null!;
    private Label lblTodayAverageOrder = null!;
    private CartesianChart chartRevenue = null!;
    private PieChart chartTopProducts = null!;

    public UC_Dashboard(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        InitializeUI();
        _ = LoadDashboardDataAsync();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(245, 245, 255);

        // TẠO LƯỚI BỐ CỤC CHÍNH (2 Dòng)
        TableLayoutPanel tlpMain = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(20)
        };
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Dòng 1: KPI Cards cao 100px
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Dòng 2: Biểu đồ chiếm phần còn lại

        // DÒNG 1: KHU VỰC KPI CARDS
        TableLayoutPanel tlpKPIs = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 3
        };
        tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333f));
        tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333f));
        tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333f));
        lblTodayRevenue = CreateKpiCard("DOANH THU HÔM NAY", "Đang tải...");
        lblTodayOrders = CreateKpiCard("SỐ ĐƠN", "Đang tải...");
        lblTodayAverageOrder = CreateKpiCard("TB ĐƠN", "Đang tải...");
        tlpKPIs.Controls.Add(lblTodayRevenue, 0, 0);
        tlpKPIs.Controls.Add(lblTodayOrders, 1, 0);
        tlpKPIs.Controls.Add(lblTodayAverageOrder, 2, 0);

        tlpMain.Controls.Add(tlpKPIs, 0, 0);

        // DÒNG 2: KHU VỰC BIỂU ĐỒ (Chia làm 2 cột)
        TableLayoutPanel tlpCharts = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 2
        };
        tlpCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65)); // Biểu đồ cột chiếm 65%
        tlpCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); // Biểu đồ tròn chiếm 35%

        // Biểu đồ Cột (Doanh thu)
        chartRevenue = new CartesianChart
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(10)
        };

        // Biểu đồ Tròn (Top Món)
        chartTopProducts = new PieChart
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(10)
        };

        tlpCharts.Controls.Add(chartRevenue, 0, 0);
        tlpCharts.Controls.Add(chartTopProducts, 1, 0);

        tlpMain.Controls.Add(tlpCharts, 0, 1);
        Controls.Add(tlpMain);
    }

    private static Label CreateKpiCard(string title, string value)
    {
        Label lbl = new()
        {
            Text = $"{title}\n{value}",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(46, 204, 113),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(8, 6, 8, 6)
        };
        return lbl;
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var taskTodayRevenue = _dashboardService.GetTodayRevenueAsync();
            var taskTodayOrders = _dashboardService.GetTodayOrderCountAsync();
            var taskTodayAverageOrder = _dashboardService.GetTodayAverageOrderAsync();

            var taskRev = _dashboardService.Get7DaysRevenueAsync();
            var taskTop = _dashboardService.GetTop5ProductsAsync();

            await Task.WhenAll(taskTodayRevenue, taskTodayOrders, taskTodayAverageOrder, taskRev, taskTop);

            lblTodayRevenue.Text =
                $"DOANH THU HÔM NAY\n{taskTodayRevenue.Result.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đ";
            lblTodayOrders.Text = $"SỐ ĐƠN\n{taskTodayOrders.Result:N0}";
            lblTodayAverageOrder.Text =
                $"TB ĐƠN\n{taskTodayAverageOrder.Result.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đ";

            var revData = taskRev.Result;
            chartRevenue.Series =
            [
                new ColumnSeries<decimal>
                {
                    Name = "Doanh thu (đ)",
                    Values = [.. revData.Select(x => x.Revenue)],
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    MaxBarWidth = 40
                }
            ];
            chartRevenue.XAxes =
            [
                new Axis { Labels = [.. revData.Select(x => x.Date.ToString("dd/MM"))] }
            ];

            var topData = taskTop.Result;
            var pieSeries = new List<ISeries>();
            foreach (var item in topData)
            {
                pieSeries.Add(new PieSeries<int>
                {
                    Name = item.ProductName,
                    Values = [item.TotalSold],
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue} ly"
                });
            }
            chartTopProducts.Series = pieSeries.ToArray();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi load Dashboard: {ex.Message}");
        }
    }
}
