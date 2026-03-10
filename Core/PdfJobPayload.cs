using CoffeePOS.Models;

namespace CoffeePOS.Core;

public class PdfJobPayload
{
    public int BillId { get; set; }
    public int TableId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BillDetail> Details { get; set; } = [];
}
