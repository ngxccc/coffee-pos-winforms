namespace CoffeePOS.Data.Repositories.Impl;

using CoffeePOS.Core;
using CoffeePOS.Models;
using Npgsql;

public class BillRepository(NpgsqlDataSource dataSource, IUserSession session) : IBillRepository
{
    public async Task<int> ProcessFullOrderAsync(int buzzerNumber, decimal totalAmount, List<BillDetail> items)
    {
        if (!session.IsLoggedIn) throw new UnauthorizedAccessException("Chưa đăng nhập không thể tạo bill!");

        using var conn = await dataSource.OpenConnectionAsync();
        using var tx = await conn.BeginTransactionAsync();

        try
        {
            string sqlBill = @"
                INSERT INTO bills (buzzer_number, user_id, status, total_amount)
                VALUES (@b, @u, 1, @total)
                RETURNING id;";

            using var cmdBill = new NpgsqlCommand(sqlBill, conn, tx);
            cmdBill.Parameters.AddWithValue("b", buzzerNumber);
            cmdBill.Parameters.AddWithValue("u", session.CurrentUser!.Id);
            cmdBill.Parameters.AddWithValue("total", totalAmount);

            int billId = Convert.ToInt32(await cmdBill.ExecuteScalarAsync());

            string sqlDetail = @"
                INSERT INTO bill_details (bill_id, product_id, product_name, quantity, price)
                VALUES (@b, @p, @n, @q, @price);";

            await using var batch = new NpgsqlBatch(conn, tx);

            foreach (var item in items)
            {
                var batchCommand = new NpgsqlBatchCommand(sqlDetail);

                batchCommand.Parameters.AddWithValue("b", billId);
                batchCommand.Parameters.AddWithValue("p", item.ProductId);
                batchCommand.Parameters.AddWithValue("n", item.ProductName);
                batchCommand.Parameters.AddWithValue("q", item.Quantity);
                batchCommand.Parameters.AddWithValue("price", item.Price);

                batch.BatchCommands.Add(batchCommand);
            }

            await batch.ExecuteNonQueryAsync();

            await tx.CommitAsync();

            return billId;
        }
        catch (Exception)
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<BillDetail>> GetBillDetailsAsync(int billId)
    {
        var list = new List<BillDetail>();

        using var conn = await dataSource.OpenConnectionAsync();

        string sql = "SELECT product_id, product_name, quantity, price, note FROM bill_details WHERE bill_id = @b ORDER BY id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("b", billId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
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

    public void CancelBill(int billId)
    {
        using var conn = dataSource.OpenConnection();
        string sql = "UPDATE bills SET is_deleted = true, updated_at = NOW() WHERE id = @id";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", billId);
        cmd.ExecuteNonQuery();
    }

    public async Task<List<Bill>> GetTodayBillsByUserAsync(int userId)
    {
        var list = new List<Bill>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"
            SELECT id, buzzer_number, total_amount, created_at
            FROM bills
            WHERE user_id = @uid
              AND created_at >= CURRENT_DATE
              AND is_deleted = false
            ORDER BY created_at DESC;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("uid", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Bill
            {
                Id = reader.GetInt32(0),
                BuzzerNumber = reader.GetInt32(1),
                TotalAmount = reader.GetDecimal(2),
                CreatedAt = reader.GetDateTime(3)
            });
        }
        return list;
    }
}
