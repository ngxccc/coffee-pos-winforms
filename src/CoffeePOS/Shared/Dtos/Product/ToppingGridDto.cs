namespace CoffeePOS.Shared.Dtos.Product;

public record ToppingGridDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public bool IsSelected { get; set; }
}
