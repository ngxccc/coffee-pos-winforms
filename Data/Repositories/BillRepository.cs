namespace CoffeePOS.Data.Repositories;

using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Npgsql;

public class BillRepository(NpgsqlDataSource dataSource) : IBillRepository
{
    private static readonly string SqlInsertBill = SqlFileLoader.Load(SqlKeys.Bill.InsertBill);
    private static readonly string SqlInsertBillDetail = SqlFileLoader.Load(SqlKeys.Bill.InsertBillDetail);
    private static readonly string SqlGetBillDetails = SqlFileLoader.Load(SqlKeys.Bill.GetBillDetails);
    private static readonly string SqlCancelBill = SqlFileLoader.Load(SqlKeys.Bill.CancelBill);
    private static readonly string SqlRestoreBill = SqlFileLoader.Load(SqlKeys.Bill.RestoreBill);
    private static readonly string SqlGetTodayBillsByUser = SqlFileLoader.Load(SqlKeys.Bill.GetTodayBillsByUser);
    private static readonly string SqlGetBillsByDateRange = SqlFileLoader.Load(SqlKeys.Bill.GetBillsByDateRange);

    public async Task<int> ProcessFullOrderAsync(CreateBillDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var tx = await conn.BeginTransactionAsync();

        try
        {
            using var cmdBill = new NpgsqlCommand(SqlInsertBill, conn, tx);
            cmdBill.Parameters.AddWithValue("b", command.BuzzerNumber);
            cmdBill.Parameters.AddWithValue("u", command.CreatedByUserId);
            cmdBill.Parameters.AddWithValue("total", command.TotalAmount);

            int billId = (await cmdBill.ExecuteScalarAsync())
                .GetRequiredIntFromScalar("BillRepository.ProcessFullOrderAsync bill id");

            await using var batch = new NpgsqlBatch(conn, tx);

            foreach (var item in command.Items)
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

    public async Task<List<BillDetailDto>> GetBillDetailsAsync(int billId)
    {
        var list = new List<BillDetailDto>();

        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetBillDetails, conn);
        cmd.Parameters.AddWithValue("b", billId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BillDetailDto(
                reader.GetRequiredInt("product_id"),
                reader.GetRequiredString("product_name"),
                reader.GetRequiredInt("quantity"),
                reader.GetRequiredDecimal("price"),
                reader.GetNullableString("note") ?? string.Empty));
        }

        return list;
    }

    public async Task CancelBillAsync(int billId, string reason)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlCancelBill, conn);
        cmd.Parameters.AddWithValue("reason", reason);
        cmd.Parameters.AddWithValue("id", billId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RestoreBillAsync(int billId)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlRestoreBill, conn);
        cmd.Parameters.AddWithValue("id", billId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId)
    {
        var list = new List<BillHistoryDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetTodayBillsByUser, conn);
        cmd.Parameters.AddWithValue("uid", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BillHistoryDto(
                reader.GetRequiredInt("id"),
                reader.GetRequiredInt("buzzer_number"),
                reader.GetRequiredDecimal("total_amount"),
                reader.GetDateOnlyAsDateTime("created_at")));
        }
        return list;
    }

    public async Task<List<BillReportDto>> GetBillsByDateRangeAsync(DateTime fromDate, DateTime toDateExclusive)
    {
        var list = new List<BillReportDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetBillsByDateRange, conn);
        cmd.Parameters.AddWithValue("from_date", fromDate);
        cmd.Parameters.AddWithValue("to_date", toDateExclusive);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BillReportDto(
                reader.GetRequiredInt("id"),
                reader.GetRequiredInt("buzzer_number"),
                reader.GetRequiredDecimal("total_amount"),
                reader.GetDateOnlyAsDateTime("created_at"),
                reader.GetRequiredString("created_by_name"),
                reader.GetRequiredBool("is_deleted"),
                reader.GetNullableDateTime("deleted_at")));
        }

        return list;
    }
}
