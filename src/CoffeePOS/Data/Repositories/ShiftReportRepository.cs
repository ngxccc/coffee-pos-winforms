using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos.ShiftReport;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ShiftReportRepository(NpgsqlDataSource dataSource) : IShiftReportRepository
{
    private static readonly string SqlGetShiftSummary = SqlFileLoader.Load(SqlKeys.ShiftReport.GetShiftSummary);
    private static readonly string SqlInsertShiftReport = SqlFileLoader.Load(SqlKeys.ShiftReport.InsertShiftReport);

    public async Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlGetShiftSummary, conn);

        cmd.Parameters.Add(new NpgsqlParameter<int>("uid", userId));
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("start", startTime));
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("end", endTime));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                reader.GetInt32(reader.GetOrdinal("total_bills")),
                reader.GetDecimal(reader.GetOrdinal("expected_cash"))
            );
        }
        return (0, 0);
    }

    public async Task SaveReportAsync(SaveShiftReportDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsertShiftReport, conn);
        cmd.Parameters.AddWithValue("user_id", command.UserId);
        cmd.Parameters.AddWithValue("start_time", command.StartTime);
        cmd.Parameters.AddWithValue("end_time", command.EndTime);
        cmd.Parameters.AddWithValue("total_bills", command.TotalBills);
        cmd.Parameters.AddWithValue("expected_cash", command.ExpectedCash);
        cmd.Parameters.AddWithValue("actual_cash", command.ActualCash);
        cmd.Parameters.AddWithValue("variance", command.Variance);
        cmd.Parameters.AddWithValue("note", string.IsNullOrWhiteSpace(command.Note) ? DBNull.Value : command.Note);

        await cmd.ExecuteNonQueryAsync();
    }
}
