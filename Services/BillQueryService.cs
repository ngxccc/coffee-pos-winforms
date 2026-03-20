using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class BillQueryService(IBillRepository billRepo) : IBillQueryService
{
    public async Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId)
    {
        var bills = await billRepo.GetTodayBillsByUserAsync(userId);
        return [.. bills.Select(b => new BillHistoryDto(
            b.Id,
            b.BuzzerNumber,
            b.TotalAmount,
            b.CreatedAt))];
    }

    public Task<List<BillDetail>> GetBillDetailsAsync(int billId)
    {
        if (billId <= 0) throw new ArgumentException("ID hóa đơn không hợp lệ!");
        return billRepo.GetBillDetailsAsync(billId);
    }
}
