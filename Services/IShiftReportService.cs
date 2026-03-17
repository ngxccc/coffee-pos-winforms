using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface IShiftReportService
{
    Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime);
    Task SaveReportAsync(ShiftReport report);
}
