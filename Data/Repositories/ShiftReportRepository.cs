using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ShiftReportRepository(NpgsqlDataSource dataSource) : IShiftReportRepository
{
    public async Task<(int TotalBills, decimal ExpectedCash)> GetShiftSummaryAsync(int userId, DateTime startTime, DateTime endTime)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"
            SELECT
                COUNT(id) as total_bills,
                COALESCE(SUM(total_amount), 0) as expected_cash
            FROM bills
            WHERE user_id = @uid
              AND created_at >= @start
              AND created_at <= @end
              AND is_deleted = false;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("uid", userId);
        cmd.Parameters.AddWithValue("start", startTime);
        cmd.Parameters.AddWithValue("end", endTime);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (reader.GetInt32(0), reader.GetDecimal(1));
        }
        return (0, 0);
    }

    public async Task SaveReportAsync(ShiftReport report)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        string sql = @"
            INSERT INTO shift_reports
            (user_id, start_time, end_time, total_bills, expected_cash, actual_cash, variance, note)
            VALUES (@u, @start, @end, @bills, @expected, @actual, @variance, @note);";

        using var cmd = new NpgsqlCommand(sql, conn);
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
