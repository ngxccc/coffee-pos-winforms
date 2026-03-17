using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface IDashboardService
{
    Task<decimal> GetTodayRevenueAsync();
    Task<int> GetTodayOrderCountAsync();
    Task<decimal> GetTodayAverageOrderAsync();
    Task<List<DailyRevenue>> Get7DaysRevenueAsync();
    Task<List<TopProduct>> GetTop5ProductsAsync();
}
