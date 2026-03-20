using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public interface IDashboardQueryService
{
    Task<TodaySummaryDto> GetTodaySummaryAsync();
    Task<List<DashboardChartDataDto>> GetRevenueChartAsync(int days = 7);
    Task<List<TopProductDto>> GetTopProductsAsync(int limit = 5);
}
