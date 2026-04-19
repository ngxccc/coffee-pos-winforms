namespace CoffeePOS.Shared.Dtos.Bill;

public record CreateBillItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string Note
);
