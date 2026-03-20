using CoffeePOS.Models;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Data.Repositories.Contracts;

public interface IBillRepository
{
    Task<int> ProcessFullOrderAsync(CreateBillDto command);
    Task<List<BillDetail>> GetBillDetailsAsync(int billId);
    Task CancelBillAsync(int billId);
    Task<List<Bill>> GetTodayBillsByUserAsync(int userId);
}
