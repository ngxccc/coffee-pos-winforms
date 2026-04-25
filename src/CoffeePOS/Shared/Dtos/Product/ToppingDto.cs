namespace CoffeePOS.Shared.Dtos.Product;

public record ToppingDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public bool IsSelected { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
}
