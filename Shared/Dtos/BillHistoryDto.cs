namespace CoffeePOS.Shared.Dtos;

public class BillHistoryDto
{
    public int Id { get; set; }
    public int BuzzerNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
