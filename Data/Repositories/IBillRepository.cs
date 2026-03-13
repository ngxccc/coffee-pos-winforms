using CoffeePOS.Models;

namespace CoffeePOS.Data.Repositories;

public interface IBillRepository
{
    Task<int> ProcessFullOrderAsync(int buzzerNumber, decimal totalAmount, List<BillDetail> items);
    List<BillDetail> GetBillDetails(int billId);
    public void CancelBill(int billId);
}
