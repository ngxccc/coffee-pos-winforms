namespace CoffeePOS.Data.Repositories.Impl;

using Npgsql;

public class BillRepository : IBillRepository
{
    private string _connStr;

    public BillRepository(string connStr)
    {
        _connStr = connStr;
    }

    public int CreateBill(int tableId)
    {
        // Code kết nối Npgsql ở đây
        // INSERT INTO Bill...
        return 0; // newBillId
    }

    public void Checkout(int billId, decimal total)
    {
        // UPDATE Bill SET status = 1...
    }
}
