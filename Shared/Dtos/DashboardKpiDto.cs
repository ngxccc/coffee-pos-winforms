namespace CoffeePOS.Shared.Dtos;

public class DashboardKpiDto
{
    public decimal TodayRevenue { get; set; }
    public int TodayOrderCount { get; set; }
    public decimal TodayAverageOrder { get; set; }
}
