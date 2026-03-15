using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IDashboardRepository
{
    Task<List<DailyRevenue>> Get7DaysRevenueAsync();
    // GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate);
}
