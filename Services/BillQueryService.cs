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
}
