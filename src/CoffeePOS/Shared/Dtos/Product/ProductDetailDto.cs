namespace CoffeePOS.Shared.Dtos.Product;

public record ProductDetailDto(
    int Id,
    string Name,
    decimal Price,
    int CategoryId,
    string ImageUrl,
    List<ProductSizeDto>? Sizes = null
);
