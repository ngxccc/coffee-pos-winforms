using System.Globalization;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Helpers;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CoffeePOS.Features.Admin;

public partial class UC_Dashboard : UserControl
{
    private readonly IDashboardQueryService _dashboardQueryService;

    public UC_Dashboard(IDashboardQueryService dashboardQueryService)
    {
        _dashboardQueryService = dashboardQueryService;

        InitializeComponent();

        // WHY: Sử dụng Discard để trigger async Task mà không làm block UI Thread lúc init
        _ = LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var taskSummary = _dashboardQueryService.GetTodaySummaryAsync();
            var taskRev = _dashboardQueryService.GetRevenueChartAsync(7);
            var taskTop = _dashboardQueryService.GetTopProductsAsync(5);

            await Task.WhenAll(taskSummary, taskRev, taskTop);

            var summary = taskSummary.Result;
            var viCulture = CultureInfo.GetCultureInfo("vi-VN");

            // Cập nhật KPI với format tiền tệ chuẩn
            _lblTodayRevenue.Text = $"{summary.Revenue.ToString("N0", viCulture)} đ";
            _lblTodayOrders.Text = $"{summary.OrderCount:N0} đơn";
            _lblTodayAverageOrder.Text = $"{summary.AverageOrder.ToString("N0", viCulture)} đ";

            // Cập nhật Chart doanh thu
            var revData = taskRev.Result;
            _chartRevenue.Series =
            [
                new ColumnSeries<decimal>
                {
                    Name = "Doanh thu",
                    Values = [.. revData.Select(x => x.Revenue)],
                    Fill = new SolidColorPaint(SKColor.Parse(ColorTranslator.ToHtml(UiTheme.BrandPrimary))),
                    MaxBarWidth = 35
                }
            ];

            _chartRevenue.XAxes =
            [
                new Axis { Labels = [.. revData.Select(x => x.Date.ToString("dd/MM"))] }
            ];

            // Cập nhật Pie Chart sản phẩm
            var topData = taskTop.Result;
            _chartTopProducts.Series = [.. topData.Select(item => new PieSeries<int>
            {
                Name = item.ProductName,
                Values = [item.TotalSold],
                DataLabelsFormatter = point => $"{point} ly"
            })];
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi load dữ liệu Dashboard: {ex.Message}", owner: this);
        }
    }
}
