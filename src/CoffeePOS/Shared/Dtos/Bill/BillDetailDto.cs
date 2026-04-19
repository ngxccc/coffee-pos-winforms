namespace CoffeePOS.Shared.Dtos.Bill;

public record BillDetailDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string Note
);
