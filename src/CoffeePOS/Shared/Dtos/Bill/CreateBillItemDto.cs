using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos.Bill;

public record CreateBillItemDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string Note { get; init; } = string.Empty;
    public BillOrderType OrderType { get; init; } = BillOrderType.DineIn;
}
