namespace CoffeePOS.Shared.Dtos.Dashboard;

public record TodaySummaryDto(
    decimal Revenue,
    int OrderCount,
    decimal AverageOrder
);
