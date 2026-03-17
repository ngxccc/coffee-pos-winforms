using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;

namespace CoffeePOS.Services;

public class ShiftReportService(IShiftReportRepository shiftReportRepo) : IShiftReportService
{
    public Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime)
        => shiftReportRepo.GetShiftSummaryAsync(userId, startTime, endTime);

    public Task SaveReportAsync(ShiftReport report)
    {
        if (report.UserId <= 0) throw new ArgumentException("Người dùng không hợp lệ!");
        if (report.EndTime < report.StartTime) throw new ArgumentException("Thời gian chốt ca không hợp lệ!");

        return shiftReportRepo.SaveReportAsync(report);
    }
}
