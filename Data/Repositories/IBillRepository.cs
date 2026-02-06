using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IBillRepository
{
    int CreateBill(int tableId);
    void Checkout(int billId, decimal total);
    int GetCurrentUnpaidBillId(int tableId);
    void AddBillDetail(int billId, int productId, string name, int qty, decimal price);
    List<BillDetail> GetBillDetails(int billId);
    DateTime? GetBillStartTime(int billId);
    public void ClearTable(int tableId);
    public int GetCurrentPaidBillId(int tableId);
}
