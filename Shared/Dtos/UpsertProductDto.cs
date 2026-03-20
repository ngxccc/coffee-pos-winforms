namespace CoffeePOS.Shared.Dtos;

public record UpsertProductDto(
    int Id,
    string Name,
    decimal Price,
    int CategoryId,
    string ImageUrl
);
