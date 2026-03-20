using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Queries;
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

    public async Task<List<BillDetailDto>> GetBillDetailsAsync(int billId)
    {
        if (billId <= 0) throw new ArgumentException("ID hóa đơn không hợp lệ!");

        var details = await billRepo.GetBillDetailsAsync(billId);
        return [.. details.Select(d => new BillDetailDto(
            d.ProductId,
            d.ProductName,
            d.Quantity,
            d.Price,
            d.Note))];
    }
}
