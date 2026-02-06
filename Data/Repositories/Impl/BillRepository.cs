namespace CoffeePOS.Data.Repositories.Impl;

using Npgsql;

public class BillRepository(NpgsqlDataSource dataSource) : IBillRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public int CreateBill(int tableId)
    {
        using var conn = _dataSource.OpenConnection();

        // Insert xong trả về luôn ID, đỡ phải SELECT MAX(id)
        string sql = @"
            INSERT INTO bills (table_id, status, created_at, total_amount)
            VALUES (@tableId, 0, NOW(), 0)
            RETURNING id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("tableId", tableId);

        // ExecuteScalar: Lấy giá trị của cột đầu tiên dòng đầu tiên (chính là ID)
        object? result = cmd.ExecuteScalar();

        return result != null ? Convert.ToInt32(result) : 0;
    }

    public void Checkout(int billId, decimal total)
    {
        using var conn = _dataSource.OpenConnection();

        // Update trạng thái thành Paid (1), lưu tổng tiền và giờ checkout
        string sql = @"
            UPDATE bills
            SET status = 1,
                total_amount = @total,
                checkout_at = NOW()
            WHERE id = @billId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("total", total);
        cmd.Parameters.AddWithValue("billId", billId);

        cmd.ExecuteNonQuery();
    }
}
