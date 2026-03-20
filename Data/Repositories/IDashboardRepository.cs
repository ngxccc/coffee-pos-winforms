using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories;

public interface IDashboardRepository
{
    Task<TodaySummaryDto> GetTodaySummaryAsync();
    Task<List<DashboardChartDataDto>> GetRevenueChartAsync(int days = 7);
    Task<List<TopProductDto>> GetTopProductsAsync(int limit = 5);
}
