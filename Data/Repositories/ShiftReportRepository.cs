using CoffeePOS.Shared.Dtos;
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
        cmd.Parameters.AddWithValue("uid", userId);
        cmd.Parameters.AddWithValue("start", startTime);
        cmd.Parameters.AddWithValue("end", endTime);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                reader.GetRequiredInt("total_bills"),
                reader.GetRequiredDecimal("expected_cash")
            );
        }
        return (0, 0);
    }

    public async Task SaveReportAsync(SaveShiftReportDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsertShiftReport, conn);
        cmd.Parameters.AddWithValue("u", command.UserId);
        cmd.Parameters.AddWithValue("start", command.StartTime);
        cmd.Parameters.AddWithValue("end", command.EndTime);
        cmd.Parameters.AddWithValue("bills", command.TotalBills);
        cmd.Parameters.AddWithValue("expected", command.ExpectedCash);
        cmd.Parameters.AddWithValue("actual", command.ActualCash);
        cmd.Parameters.AddWithValue("variance", command.Variance);
        cmd.Parameters.AddWithValue("note", string.IsNullOrWhiteSpace(command.Note) ? DBNull.Value : command.Note);

        await cmd.ExecuteNonQueryAsync();
    }
}
