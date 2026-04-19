namespace CoffeePOS.Shared.Dtos;

public record ProductSizeDto(
    int ProductId,
    string SizeName,
    decimal PriceAdjustment
);
