namespace CoffeePOS.Shared.Dtos;

public record ProductDetailDto(
    int Id,
    string Name,
    decimal Price,
    int CategoryId,
    string ImageUrl
);
