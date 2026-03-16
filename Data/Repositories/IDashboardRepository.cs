using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IDashboardRepository
{
    Task<decimal> GetTodayRevenueAsync();
    Task<int> GetTodayOrderCountAsync();
    Task<decimal> GetTodayAverageOrderAsync();
    Task<List<DailyRevenue>> Get7DaysRevenueAsync();
    // GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<TopProduct>> GetTop5ProductsAsync();
}
