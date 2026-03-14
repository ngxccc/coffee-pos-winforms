using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IBillRepository
{
    Task<int> ProcessFullOrderAsync(int buzzerNumber, decimal totalAmount, List<BillDetail> items);
    Task<List<BillDetail>> GetBillDetailsAsync(int billId);
    public void CancelBill(int billId);
    Task<List<Bill>> GetTodayBillsByUserAsync(int userId);
}
