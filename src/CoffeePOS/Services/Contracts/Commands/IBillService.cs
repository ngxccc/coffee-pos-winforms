
using CoffeePOS.Shared.Dtos.Bill;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IBillService
{
    Task<int> ProcessFullOrderAsync(CreateBillDto command);
    Task CancelBillAsync(int billId, string reason);
    Task RestoreBillAsync(int billId);
}
