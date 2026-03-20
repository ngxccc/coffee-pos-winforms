using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services.Contracts.Queries;

public interface IBillQueryService
{
    Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId);
    Task<List<BillDetailDto>> GetBillDetailsAsync(int billId);
}
