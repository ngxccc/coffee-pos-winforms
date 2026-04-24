
using CoffeePOS.Shared.Dtos.Bill;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IBillRepository
{
    Task<int> ProcessFullOrderAsync(CreateBillDto command);
    Task<List<BillDetailDto>> GetBillDetailsAsync(int billId);
    Task CancelBillAsync(int billId, string reason);
    Task RestoreBillAsync(int billId);
    Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId);
    Task<List<BillReportDto>> GetBillsByDateRangeAsync(DateTime fromDate, DateTime toDateExclusive);
}
