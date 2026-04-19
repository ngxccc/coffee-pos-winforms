
using CoffeePOS.Shared.Dtos.Dashboard;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IDashboardRepository
{
    Task<TodaySummaryDto> GetTodaySummaryAsync();
    Task<List<DashboardChartDataDto>> GetRevenueChartAsync(int days = 7);
    Task<List<TopProductDto>> GetTopProductsAsync(int limit = 5);
}
