using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public interface IBillService
{
    Task<int> ProcessFullOrderAsync(CreateBillDto command);
    Task CancelBillAsync(int billId);
}
