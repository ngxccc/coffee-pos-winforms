using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Core;

public interface IPdfPayload { }

public class BillPrintPayload : IPdfPayload
{
    public int BillId { get; set; }
    public int BuzzerNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BillDetailDto> Details { get; set; } = [];
    public bool IsReprint { get; set; }
}

public class ShiftReportPrintPayload : IPdfPayload
{
    public string CashierName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalBills { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal ActualCash { get; set; }
    public decimal Variance { get; set; }
    public string Note { get; set; } = "";
}
