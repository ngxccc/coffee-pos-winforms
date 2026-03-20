namespace CoffeePOS.Shared.Dtos;

public record TodaySummaryDto(
    decimal Revenue,
    int OrderCount,
    decimal AverageOrder
);
