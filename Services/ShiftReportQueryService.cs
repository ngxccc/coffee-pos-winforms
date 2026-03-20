using CoffeePOS.Data.Repositories;

namespace CoffeePOS.Services;

public class ShiftReportQueryService(IShiftReportRepository shiftReportRepo) : IShiftReportQueryService
{
    public Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime)
        => shiftReportRepo.GetShiftSummaryAsync(userId, startTime, endTime);
}
