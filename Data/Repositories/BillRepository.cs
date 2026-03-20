namespace CoffeePOS.Data.Repositories;

using CoffeePOS.Core;
using CoffeePOS.Models;
using CoffeePOS.Shared.Helpers;
using Npgsql;

public class BillRepository(NpgsqlDataSource dataSource, IUserSession session) : IBillRepository
{
    private static readonly string SqlInsertBill = SqlFileLoader.Load(SqlKeys.Bill.InsertBill);
    private static readonly string SqlInsertBillDetail = SqlFileLoader.Load(SqlKeys.Bill.InsertBillDetail);
    private static readonly string SqlGetBillDetails = SqlFileLoader.Load(SqlKeys.Bill.GetBillDetails);
    private static readonly string SqlCancelBill = SqlFileLoader.Load(SqlKeys.Bill.CancelBill);
    private static readonly string SqlGetTodayBillsByUser = SqlFileLoader.Load(SqlKeys.Bill.GetTodayBillsByUser);

    public async Task<int> ProcessFullOrderAsync(int buzzerNumber, decimal totalAmount, List<BillDetail> items)
    {
        if (!session.IsLoggedIn) throw new UnauthorizedAccessException("Chưa đăng nhập không thể tạo bill!");

        using var conn = await dataSource.OpenConnectionAsync();
        using var tx = await conn.BeginTransactionAsync();

        try
        {
            using var cmdBill = new NpgsqlCommand(SqlInsertBill, conn, tx);
            cmdBill.Parameters.AddWithValue("b", buzzerNumber);
            cmdBill.Parameters.AddWithValue("u", session.CurrentUser!.Id);
            cmdBill.Parameters.AddWithValue("total", totalAmount);

            int billId = (await cmdBill.ExecuteScalarAsync())
                .GetRequiredIntFromScalar("BillRepository.ProcessFullOrderAsync bill id");

            await using var batch = new NpgsqlBatch(conn, tx);

            foreach (var item in items)
            {
                var batchCommand = new NpgsqlBatchCommand(SqlInsertBillDetail);

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

        using var cmd = new NpgsqlCommand(SqlGetBillDetails, conn);
        cmd.Parameters.AddWithValue("b", billId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BillDetail
            {
                ProductId = reader.GetRequiredInt("product_id"),
                ProductName = reader.GetRequiredString("product_name"),
                Quantity = reader.GetRequiredInt("quantity"),
                Price = reader.GetRequiredDecimal("price"),
                Note = reader.GetNullableString("note") ?? string.Empty
            });
        }

        return list;
    }

    public async Task CancelBillAsync(int billId)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlCancelBill, conn);
        cmd.Parameters.AddWithValue("id", billId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<Bill>> GetTodayBillsByUserAsync(int userId)
    {
        var list = new List<Bill>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetTodayBillsByUser, conn);
        cmd.Parameters.AddWithValue("uid", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Bill
            {
                Id = reader.GetRequiredInt("id"),
                BuzzerNumber = reader.GetRequiredInt("buzzer_number"),
                TotalAmount = reader.GetRequiredDecimal("total_amount"),
                CreatedAt = reader.GetDateOnlyAsDateTime("created_at")
            });
        }
        return list;
    }
}
