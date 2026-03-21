using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IBillRepository
{
    Task<int> ProcessFullOrderAsync(CreateBillDto command);
    Task<List<BillDetailDto>> GetBillDetailsAsync(int billId);
    Task CancelBillAsync(int billId);
    Task RestoreBillAsync(int billId);
    Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId);
    Task<List<BillReportDto>> GetBillsByDateRangeAsync(DateTime fromDate, DateTime toDateExclusive);
}
