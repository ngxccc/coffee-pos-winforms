using CoffeePOS.Data.Repositories;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class DashboardQueryService(IDashboardRepository dashboardRepo) : IDashboardQueryService
{
    public async Task<DashboardKpiDto> GetTodayKpisAsync()
    {
        var todayRevenue = await dashboardRepo.GetTodayRevenueAsync();
        var todayOrders = await dashboardRepo.GetTodayOrderCountAsync();
        var todayAverage = await dashboardRepo.GetTodayAverageOrderAsync();

        return new DashboardKpiDto
        {
            TodayRevenue = todayRevenue,
            TodayOrderCount = todayOrders,
            TodayAverageOrder = todayAverage
        };
    }

    public async Task<List<DashboardChartDataDto>> Get7DaysRevenueChartAsync()
    {
        var data = await dashboardRepo.Get7DaysRevenueAsync();
        return [.. data.Select(d => new DashboardChartDataDto
        {
            Date = d.Date,
            TotalBills = d.TotalBills,
            Revenue = d.Revenue
        })];
    }

    public async Task<List<TopProductDto>> GetTop5ProductsAsync()
    {
        var data = await dashboardRepo.GetTop5ProductsAsync();
        return [.. data.Select(t => new TopProductDto
        {
            ProductName = t.ProductName,
            TotalSold = t.TotalSold
        })];
    }
}
