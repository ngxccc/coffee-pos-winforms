using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos.ShiftReport;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ShiftReportRepository(NpgsqlDataSource dataSource) : IShiftReportRepository
{
    private static readonly string SqlGetShiftSummary = SqlFileLoader.Load(SqlKeys.ShiftReport.GetShiftSummary);
    private static readonly string SqlInsertShiftReport = SqlFileLoader.Load(SqlKeys.ShiftReport.InsertShiftReport);
    private static readonly string SqlGetAllShiftReports = SqlFileLoader.Load(SqlKeys.ShiftReport.GetAllShiftReports);

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
                reader.GetRequired<int>("total_bills"),
                reader.GetRequired<decimal>("expected_cash")
            );
        }
        return (0, 0);
    }

    public async Task InsertReportAsync(UpsertShiftReportDto dto)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsertShiftReport, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("user_id", dto.UserId));
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("start_time", dto.StartTime));
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("end_time", dto.EndTime));
        cmd.Parameters.Add(new NpgsqlParameter<int>("total_bills", dto.TotalBills));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("starting_cash", dto.StartingCash));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("expected_cash", dto.ExpectedCash));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("actual_cash", dto.ActualCash));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("difference", dto.Difference));
        cmd.Parameters.Add(new NpgsqlParameter<string?>("note", string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note));

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<ShiftReportDto>> GetAllShiftReportsAsync()
    {
        var result = new List<ShiftReportDto>();

        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlGetAllShiftReports, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new ShiftReportDto(
                reader.GetRequired<int>("id"),
                reader.GetRequired<string>("cashier_name"),
                reader.GetRequired<DateTime>("start_time"),
                reader.GetRequired<DateTime>("end_time"),
                reader.GetRequired<int>("total_bills"),
                reader.GetRequired<decimal>("starting_cash"),
                reader.GetRequired<decimal>("expected_cash"),
                reader.GetRequired<decimal>("actual_cash"),
                reader.GetRequired<decimal>("difference"),
                reader.GetNullable<string>("note")
            ));
        }
        return result;
    }
}
