using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;

namespace CoffeePOS.Services;

public class BillService(IBillRepository billRepo) : IBillService
{
    public Task<int> ProcessFullOrderAsync(int buzzerNumber, decimal totalAmount, List<BillDetail> items)
    {
        if (buzzerNumber <= 0) throw new ArgumentException("Số thẻ rung phải lớn hơn 0!");
        if (items.Count == 0) throw new ArgumentException("Không thể tạo hóa đơn trống!");
        if (totalAmount <= 0) throw new ArgumentException("Tổng tiền hóa đơn không hợp lệ!");

        return billRepo.ProcessFullOrderAsync(buzzerNumber, totalAmount, items);
    }

    public Task<List<BillDetail>> GetBillDetailsAsync(int billId) => billRepo.GetBillDetailsAsync(billId);

    public Task<List<Bill>> GetTodayBillsByUserAsync(int userId) => billRepo.GetTodayBillsByUserAsync(userId);

    public Task CancelBillAsync(int billId) => billRepo.CancelBillAsync(billId);
}
