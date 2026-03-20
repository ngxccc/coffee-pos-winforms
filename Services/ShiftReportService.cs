using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class ShiftReportService(IShiftReportRepository shiftReportRepo) : IShiftReportService
{
    public Task SaveReportAsync(SaveShiftReportDto command)
    {
        if (command.UserId <= 0) throw new ArgumentException("Người dùng không hợp lệ!");
        if (command.EndTime < command.StartTime) throw new ArgumentException("Thời gian chốt ca không hợp lệ!");
        if (command.TotalBills < 0) throw new ArgumentException("Số hóa đơn không hợp lệ!");
        if (command.ExpectedCash < 0 || command.ActualCash < 0) throw new ArgumentException("Số tiền không hợp lệ!");

        return shiftReportRepo.SaveReportAsync(command);
    }
}
