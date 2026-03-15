using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IShiftReportRepository
{
    Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime);
    Task SaveReportAsync(ShiftReport report);
}
