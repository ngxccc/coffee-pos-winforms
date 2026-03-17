using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;

namespace CoffeePOS.Services;

public class DashboardService(IDashboardRepository dashboardRepo) : IDashboardService
{
    public Task<decimal> GetTodayRevenueAsync() => dashboardRepo.GetTodayRevenueAsync();

    public Task<int> GetTodayOrderCountAsync() => dashboardRepo.GetTodayOrderCountAsync();

    public Task<decimal> GetTodayAverageOrderAsync() => dashboardRepo.GetTodayAverageOrderAsync();

    public Task<List<DailyRevenue>> Get7DaysRevenueAsync() => dashboardRepo.Get7DaysRevenueAsync();

    public Task<List<TopProduct>> GetTop5ProductsAsync() => dashboardRepo.GetTop5ProductsAsync();
}
