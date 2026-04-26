namespace CoffeePOS.Data.Repositories;

using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Enums;
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
            cmdBill.Parameters.Add(new NpgsqlParameter<int>("buzzer_number", command.BuzzerNumber));
            cmdBill.Parameters.Add(new NpgsqlParameter<int>("user_id", command.CreatedByUserId));
            cmdBill.Parameters.Add(new NpgsqlParameter<decimal>("total_amount", command.TotalAmount));

            int billId = (await cmdBill.ExecuteScalarAsync())
                .GetRequiredIntFromScalar("BillRepository.ProcessFullOrderAsync bill id");

            await using var batch = new NpgsqlBatch(conn, tx);

            foreach (var item in command.Items)
            {
                var batchCommand = new NpgsqlBatchCommand(SqlInsertBillDetail);

                batchCommand.Parameters.Add(new NpgsqlParameter<int>("bill_id", billId));
                batchCommand.Parameters.Add(new NpgsqlParameter<int>("product_id", item.ProductId));
                batchCommand.Parameters.Add(new NpgsqlParameter<string>("product_name", item.ProductName));
                batchCommand.Parameters.Add(new NpgsqlParameter<int>("quantity", item.Quantity));
                batchCommand.Parameters.Add(new NpgsqlParameter<BillOrderType>("order_type", item.OrderType));
                batchCommand.Parameters.Add(new NpgsqlParameter<decimal>("base_price", item.Price));
                batchCommand.Parameters.Add(new NpgsqlParameter<string>("note", item.Note));

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
        cmd.Parameters.Add(new NpgsqlParameter<int>("b", billId));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BillDetailDto
            {
                ProductId = reader.GetRequired<int>("product_id"),
                ProductName = reader.GetRequired<string>("product_name"),
                Quantity = reader.GetRequired<int>("quantity"),
                Price = reader.GetRequired<decimal>("base_price"),
                Note = reader.GetNullable<string>("note") ?? string.Empty
            });
        }

        return list;
    }

    public async Task CancelBillAsync(int billId, int userId, string reason)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlCancelBill, conn);
        cmd.Parameters.Add(new NpgsqlParameter<string>("cancel_reason", reason));
        cmd.Parameters.Add(new NpgsqlParameter<int>("id", billId));
        cmd.Parameters.Add(new NpgsqlParameter<int>("canceled_by", userId));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RestoreBillAsync(int billId)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlRestoreBill, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("id", billId));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<BillHistoryDto>> GetTodayBillsByUserAsync(int userId)
    {
        var list = new List<BillHistoryDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetTodayBillsByUser, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("uid", userId));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BillHistoryDto(
                reader.GetRequired<int>("id"),
                reader.GetRequired<int>("buzzer_number"),
                reader.GetRequired<int>("total_items"),
                reader.GetRequired<decimal>("total_amount"),
                reader.GetRequired<BillStatus>("status"),
                reader.GetRequired<DateTime>("created_at"),
                reader.GetNullable<DateTime>("canceled_at")));
        }
        return list;
    }

    public async Task<List<BillReportDto>> GetBillsByDateRangeAsync(DateTime fromDate, DateTime toDateExclusive)
    {
        var list = new List<BillReportDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetBillsByDateRange, conn);
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("from_date", fromDate));
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("to_date", toDateExclusive));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BillReportDto(
                reader.GetRequired<int>("id"),
                reader.GetRequired<int>("buzzer_number"),
                reader.GetRequired<decimal>("total_amount"),
                reader.GetRequired<BillStatus>("status"),
                reader.GetRequired<string>("created_by_name"),
                reader.GetNullable<string>("canceled_by_name"),
                reader.GetRequired<DateTime>("created_at"),
                reader.GetNullable<DateTime>("canceled_at")));
        }

        return list;
    }
}
