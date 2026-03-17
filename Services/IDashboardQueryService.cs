using CoffeePOS.Models;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public interface IDashboardQueryService
{
    Task<DashboardKpiDto> GetTodayKpisAsync();
    Task<List<DashboardChartDataDto>> Get7DaysRevenueChartAsync();
    Task<List<TopProductDto>> GetTop5ProductsAsync();
}
