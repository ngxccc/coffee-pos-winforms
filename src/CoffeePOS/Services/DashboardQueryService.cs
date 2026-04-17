using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class DashboardQueryService(IDashboardRepository dashboardRepo) : IDashboardQueryService
{
    public Task<TodaySummaryDto> GetTodaySummaryAsync() => dashboardRepo.GetTodaySummaryAsync();

    public Task<List<DashboardChartDataDto>> GetRevenueChartAsync(int days = 7) =>
        dashboardRepo.GetRevenueChartAsync(days);

    public Task<List<TopProductDto>> GetTopProductsAsync(int limit = 5) =>
        dashboardRepo.GetTopProductsAsync(limit);
}
