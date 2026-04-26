using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos.Bill;

public record CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string SizeName { get; set; } = "M";
    public BillOrderType OrderType { get; init; } = BillOrderType.DineIn;
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public List<ToppingDto> Toppings { get; set; } = [];
    public string Note { get; set; } = "";
    public string DisplayName
    {
        get
        {
            return $"{ProductName} ({SizeName})";
        }
    }
    public int Quantity { get; set; } = 1;
    public decimal TotalLinePrice => (BasePrice + Toppings.Sum(t => t.Price)) * Quantity;
}
