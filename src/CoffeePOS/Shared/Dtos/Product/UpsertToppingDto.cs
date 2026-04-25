namespace CoffeePOS.Shared.Dtos.Product;

public record UpsertToppingDto(
    int Id,
    string Name,
    decimal Price
);
