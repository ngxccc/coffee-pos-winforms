namespace CoffeePOS.Shared.Dtos.Product;

public record UpsertProductSizeDto(
    int Id,
    int ProductId,
    string SizeName,
    decimal PriceAdjustment
);
