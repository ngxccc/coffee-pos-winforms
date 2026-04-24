
using CoffeePOS.Shared.Dtos.Dashboard;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IDashboardQueryService
{
    Task<TodaySummaryDto> GetTodaySummaryAsync();
    Task<List<DashboardChartDataDto>> GetRevenueChartAsync(int days = 7);
    Task<List<TopProductDto>> GetTopProductsAsync(int limit = 5);
}
