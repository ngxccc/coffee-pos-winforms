namespace CoffeePOS.Data.Repositories.Impl;

using CoffeePOS.Models;
using Npgsql;

public class BillRepository(NpgsqlDataSource dataSource) : IBillRepository
{
    public async Task<int> ProcessFullOrderAsync(int buzzerNumber, decimal totalAmount, List<BillDetail> items)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var tx = await conn.BeginTransactionAsync();

        try
        {
            string sqlBill = @"
                INSERT INTO bills (buzzer_number, status, total_amount)
                VALUES (@b, 1, @total)
                RETURNING id;";

            using var cmdBill = new NpgsqlCommand(sqlBill, conn, tx);
            cmdBill.Parameters.AddWithValue("b", buzzerNumber);
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

    public void CancelBill(int billId)
    {
        using var conn = dataSource.OpenConnection();
        string sql = "UPDATE bills SET is_deleted = true, updated_at = NOW() WHERE id = @id";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", billId);
        cmd.ExecuteNonQuery();
    }
}
