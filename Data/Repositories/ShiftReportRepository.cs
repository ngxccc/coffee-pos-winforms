using CoffeePOS.Models;
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

    public async Task SaveReportAsync(ShiftReport report)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsertShiftReport, conn);
        cmd.Parameters.AddWithValue("u", report.UserId);
        cmd.Parameters.AddWithValue("start", report.StartTime);
        cmd.Parameters.AddWithValue("end", report.EndTime);
        cmd.Parameters.AddWithValue("bills", report.TotalBills);
        cmd.Parameters.AddWithValue("expected", report.ExpectedCash);
        cmd.Parameters.AddWithValue("actual", report.ActualCash);
        cmd.Parameters.AddWithValue("variance", report.Variance);
        cmd.Parameters.AddWithValue("note", string.IsNullOrWhiteSpace(report.Note) ? DBNull.Value : report.Note);

        await cmd.ExecuteNonQueryAsync();
    }
}
