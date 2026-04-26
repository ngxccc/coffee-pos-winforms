using CoffeePOS.Shared.Dtos.ShiftReport;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IShiftReportQueryService
{
    Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime);
    Task<List<ShiftReportDto>> GetAllShiftReportsAsync();
}
