namespace CoffeePOS.Services;

public interface IShiftReportQueryService
{
    Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime);
}
