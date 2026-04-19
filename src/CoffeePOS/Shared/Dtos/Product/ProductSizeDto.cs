namespace CoffeePOS.Shared.Dtos.Product;

public record ProductSizeDto(
    int ProductId,
    string SizeName,
    decimal PriceAdjustment
);
