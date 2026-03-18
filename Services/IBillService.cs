using CoffeePOS.Models;

namespace CoffeePOS.Services;

public interface IBillService
{
    Task<int> ProcessFullOrderAsync(int buzzerNumber, decimal totalAmount, List<BillDetail> items);
    Task<List<BillDetail>> GetBillDetailsAsync(int billId);
    Task<List<Bill>> GetTodayBillsByUserAsync(int userId);
    Task CancelBillAsync(int billId);
}
