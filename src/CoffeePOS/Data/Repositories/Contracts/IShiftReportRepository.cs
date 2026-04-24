using CoffeePOS.Shared.Dtos.ShiftReport;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IShiftReportRepository
{
    Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime);
    Task SaveReportAsync(SaveShiftReportDto command);
}
