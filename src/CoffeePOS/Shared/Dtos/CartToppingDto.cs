namespace CoffeePOS.Shared.Dtos;

public record CartToppingDto(
    int ToppingId,
    string ToppingName,
    decimal Price
);
