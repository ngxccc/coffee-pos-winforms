namespace CoffeePOS.Shared.Dtos;

public record CreateBillItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string Note
);
