using CoffeePOS.Data.Repositories;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public class BillQueryService(IBillRepository billRepo) : IBillQueryService
{
    public async Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId)
    {
        var bills = await billRepo.GetTodayBillsByUserAsync(userId);
        return [.. bills.Select(b => new BillHistoryDto
        {
            Id = b.Id,
            BuzzerNumber = b.BuzzerNumber,
            TotalAmount = b.TotalAmount,
            CreatedAt = b.CreatedAt
        })];
    }
}
