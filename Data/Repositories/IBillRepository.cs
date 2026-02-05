namespace CoffeePOS.Data.Repositories;

public interface IBillRepository
{
    int CreateBill(int tableId);
    void Checkout(int billId, decimal total);
}
