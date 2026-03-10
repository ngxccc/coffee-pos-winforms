using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IBillRepository
{
    Task<int> CreateBillAsync(int tableId);
    Task CheckoutAsync(int billId, decimal total);
    int GetCurrentUnpaidBillId(int tableId);
    Task AddBillDetailAsync(int billId, int productId, string name, int qty, decimal price);
    List<BillDetail> GetBillDetails(int billId);
    DateTime? GetBillStartTime(int billId);
    public void ClearTable(int tableId);
    public int GetCurrentPaidBillId(int tableId);
}
