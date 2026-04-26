using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Commands;

using CoffeePOS.Shared.Dtos.ShiftReport;

namespace CoffeePOS.Services;

public class ShiftReportService(IShiftReportRepository shiftReportRepo) : IShiftReportService
{
    public Task SaveReportAsync(UpsertShiftReportDto dto)
    {
        if (dto.UserId <= 0) throw new ArgumentException("Người dùng không hợp lệ!");
        if (dto.EndTime < dto.StartTime) throw new ArgumentException("Thời gian chốt ca không hợp lệ!");
        if (dto.TotalBills < 0) throw new ArgumentException("Số hóa đơn không hợp lệ!");
        if (dto.ExpectedCash < 0 || dto.ActualCash < 0) throw new ArgumentException("Số tiền không hợp lệ!");
        if (dto.Difference != 0 && string.IsNullOrWhiteSpace(dto.Note))
            throw new ArgumentException("Ca có lệch tiền nên bắt buộc nhập ghi chú lý do!");

        return shiftReportRepo.InsertReportAsync(dto);
    }
}
