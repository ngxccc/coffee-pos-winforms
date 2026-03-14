namespace CoffeePOS.Models;

public class Bill
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BuzzerNumber { get; set; }
    public int OrderType { get; set; } = 1;
    public decimal TotalAmount { get; set; }
    public int Status { get; set; } = 1;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
