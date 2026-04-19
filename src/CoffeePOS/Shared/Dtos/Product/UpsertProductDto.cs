namespace CoffeePOS.Shared.Dtos.Product;

public record UpsertProductDto(
    int Id,
    string Name,
    decimal Price,
    int CategoryId,
    string ImageUrl
);
