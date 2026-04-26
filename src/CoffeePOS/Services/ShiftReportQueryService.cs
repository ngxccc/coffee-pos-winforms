
using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.ShiftReport;

namespace CoffeePOS.Services;

public class ShiftReportQueryService(IShiftReportRepository shiftReportRepo) : IShiftReportQueryService
{
    public Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime)
        => shiftReportRepo.GetShiftSummaryAsync(userId, startTime, endTime);

    public async Task<List<ShiftReportDto>> GetAllShiftReportsAsync()
        => await shiftReportRepo.GetAllShiftReportsAsync();
}
