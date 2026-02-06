namespace CoffeePOS.Data.Repositories.Impl;

using CoffeePOS.Models;
using Npgsql;

public class BillRepository(NpgsqlDataSource dataSource) : IBillRepository
{
    public int CreateBill(int tableId)
    {
        using var conn = dataSource.OpenConnection();

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
        using var conn = dataSource.OpenConnection();

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

    public int GetCurrentUnpaidBillId(int tableId)
    {
        using var conn = dataSource.OpenConnection();
        string sql = "SELECT id FROM bills WHERE table_id = @t AND status = 0 ORDER BY id DESC LIMIT 1";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("t", tableId);
        var res = cmd.ExecuteScalar();
        return res == null ? 0 : (int)res;
    }

    public void AddBillDetail(int billId, int productId, string name, int qty, decimal price)
    {
        using var conn = dataSource.OpenConnection();

        // Kiểm tra món đã tồn tại chưa để cộng dồn
        string sql = @"
            INSERT INTO bill_details (bill_id, product_id, product_name, quantity, price)
            VALUES (@b, @p, @n, @q, @price)
            ON CONFLICT (bill_id, product_id)
            DO UPDATE SET
                quantity = bill_details.quantity + EXCLUDED.quantity;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("b", billId);
        cmd.Parameters.AddWithValue("p", productId);
        cmd.Parameters.AddWithValue("n", name);
        cmd.Parameters.AddWithValue("q", qty);
        cmd.Parameters.AddWithValue("price", price);

        cmd.ExecuteNonQuery();
    }

    public List<BillDetail> GetBillDetails(int billId)
    {
        var list = new List<BillDetail>();
        using var conn = dataSource.OpenConnection();
        string sql = "SELECT product_id, product_name, quantity, price, note FROM bill_details WHERE bill_id = @b ORDER BY id";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("b", billId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new BillDetail
            {
                ProductId = reader.GetInt32(0),
                ProductName = reader.GetString(1),
                Quantity = reader.GetInt32(2),
                Price = reader.GetDecimal(3),
                Note = reader.IsDBNull(4) ? "" : reader.GetString(4)
            });
        }
        return list;
    }

    public DateTime? GetBillStartTime(int billId)
    {
        using var conn = dataSource.OpenConnection();
        string sql = "SELECT created_at FROM bills WHERE id = @id";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", billId);
        var res = cmd.ExecuteScalar();
        return res == DBNull.Value ? null : (DateTime?)res;
    }
}
