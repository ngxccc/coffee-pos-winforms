using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos;

public record CartItemDto
{
    [Browsable(false)] public int ProductId { get; set; }
    [Browsable(false)] public string ProductName { get; set; } = "";
    [Browsable(false)] public string SizeName { get; set; } = "M";
    [Browsable(false)] public decimal BasePrice { get; set; }
    [Browsable(false)] public string? ImageUrl { get; set; }
    [Browsable(false)] public List<ToppingGridDto> Toppings { get; set; } = [];

    [DisplayName("Tên Món & Tùy Chọn")]
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

    [DisplayName("SL")]
    public int Quantity { get; set; } = 1;

    [DisplayName("Thành Tiền")]
    public decimal TotalLinePrice => (BasePrice + Toppings.Sum(t => t.Price)) * Quantity;
}
