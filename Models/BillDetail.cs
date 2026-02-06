namespace CoffeePOS.Models;

public class BillDetail
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Note { get; set; } = "";
}
