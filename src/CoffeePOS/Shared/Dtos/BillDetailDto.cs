namespace CoffeePOS.Shared.Dtos;

public record BillDetailDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string Note
);
