namespace CoffeePOS.Models;

public class DailyRevenue
{
    public DateTime Date { get; set; }
    public int TotalBills { get; set; }
    public decimal Revenue { get; set; }
}
