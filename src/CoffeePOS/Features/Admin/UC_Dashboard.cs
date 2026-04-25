using System.Globalization;
using AntdUI;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_Dashboard : UserControl
{
    private readonly IDashboardQueryService _dashboardQueryService;

    public UC_Dashboard(IDashboardQueryService dashboardQueryService)
    {
        _dashboardQueryService = dashboardQueryService;

        InitializeComponent();

        Load += async (s, e) => await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        await Spin.open(this, async cfg =>
        {
            try
            {
                var taskSummary = _dashboardQueryService.GetTodaySummaryAsync();
                var taskRev = _dashboardQueryService.GetRevenueChartAsync(7);
                var taskTop = _dashboardQueryService.GetTopProductsAsync(5);

                await Task.WhenAll(taskSummary, taskRev, taskTop);

                var summary = taskSummary.Result;
                var revData = taskRev.Result;
                var topData = taskTop.Result;

                Invoke(() =>
                {
                    var viCulture = CultureInfo.GetCultureInfo("vi-VN");

                    _lblTodayRevenue.Text = $"{summary.Revenue.ToString("N0", viCulture)} đ";
                    _lblTodayOrders.Text = $"{summary.OrderCount:N0} đơn";
                    _lblTodayAverageOrder.Text = $"{summary.AverageOrder.ToString("N0", viCulture)} đ";

                    // REVENUE CHART
                    var revDataset = new ChartDataset("Doanh thu", Color.FromArgb(24, 144, 255))
                    {
                        BorderColor = Color.FromArgb(24, 144, 255),
                        BorderWidth = 2,
                        Opacity = 0.8f
                    };

                    for (int i = 0; i < revData.Count; ++i)
                    {
                        var item = revData[i];
                        revDataset.AddPoint(item.Date.ToString("dd/MM"), i + 1, (double)item.Revenue);
                    }

                    _chartRevenue.AddDataset(revDataset);

                    // PRODUCTS PIE CHART
                    var topDataset = new ChartDataset("Sản phẩm bán chạy", Color.FromArgb(250, 173, 20));

                    foreach (var item in topData)
                        topDataset.AddPoint(item.ProductName, 0, item.TotalSold);

                    _chartTopProducts.AddDataset(topDataset);
                });
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error($"Lỗi load dữ liệu Dashboard: {ex.Message}", owner: this));
            }
        });
    }
}
