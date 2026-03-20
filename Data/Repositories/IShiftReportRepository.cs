using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories;

public interface IShiftReportRepository
{
    Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime);
    Task SaveReportAsync(SaveShiftReportDto command);
}
