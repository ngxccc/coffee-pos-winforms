using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Shared.Dtos.Bill;

public record CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string SizeName { get; set; } = "M";
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public List<ToppingDto> Toppings { get; set; } = [];
    public string Note { get; set; } = "";
    public string DisplayName
    {
        get
        {
            string name = $"{ProductName} ({SizeName})";
            if (Toppings.Count > 0)
            {
                string toppingStr = string.Join(" + ", Toppings.Select(t => t.Name));
                name += $" + {toppingStr}";
            }
            return name;
        }
    }
    public int Quantity { get; set; } = 1;
    public decimal TotalLinePrice => (BasePrice + Toppings.Sum(t => t.Price)) * Quantity;
}
