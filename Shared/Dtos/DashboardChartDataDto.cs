namespace CoffeePOS.Shared.Dtos;

public record DashboardChartDataDto(
    DateTime Date,
    int TotalBills,
    decimal Revenue
);
