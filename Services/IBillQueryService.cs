using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public interface IBillQueryService
{
    Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId);
}
