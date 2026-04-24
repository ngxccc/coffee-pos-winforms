namespace CoffeePOS.Shared.Dtos.Dashboard;

public record DashboardChartDataDto(
    DateTime Date,
    int TotalBills,
    decimal Revenue
);
