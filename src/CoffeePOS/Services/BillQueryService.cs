using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class BillQueryService(IBillRepository billRepo) : IBillQueryService
{
    public Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId)
        => billRepo.GetTodayBillsByUserAsync(userId);

    public Task<List<BillDetailDto>> GetBillDetailsAsync(int billId)
    {
        if (billId <= 0) throw new ArgumentException("ID hóa đơn không hợp lệ!");
        return billRepo.GetBillDetailsAsync(billId);
    }

    public Task<List<BillReportDto>> GetBillsByDateRangeAsync(DateOnly fromDate, DateOnly toDate)
    {
        if (fromDate > toDate)
            throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc!");

        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        return billRepo.GetBillsByDateRangeAsync(fromDateTime, toDateExclusive);
    }
}
