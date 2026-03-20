using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class DashboardRepository(NpgsqlDataSource dataSource) : IDashboardRepository
{
    private static readonly string SqlGetTodaySummary = SqlFileLoader.Load(SqlKeys.Dashboard.GetTodaySummary);
    private static readonly string SqlGetRevenueChart = SqlFileLoader.Load(SqlKeys.Dashboard.GetRevenueChart);
    private static readonly string SqlGetTopProducts = SqlFileLoader.Load(SqlKeys.Dashboard.GetTopProducts);

    public async Task<TodaySummaryDto> GetTodaySummaryAsync()
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetTodaySummary, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            decimal revenue = Convert.ToDecimal(reader["revenue"]);
            int count = Convert.ToInt32(reader["order_count"]);
            decimal avgOrder = Convert.ToDecimal(reader["avg_order"]);

            return new TodaySummaryDto(revenue, count, avgOrder);
        }

        return new TodaySummaryDto(0, 0, 0);
    }

    public async Task<List<DashboardChartDataDto>> GetRevenueChartAsync(int days = 7)
    {
        if (days < 1) throw new ArgumentOutOfRangeException(nameof(days), "Days must be greater than 0.");

        var list = new List<DashboardChartDataDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetRevenueChart, conn);
        cmd.Parameters.AddWithValue("days", days);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var reportDate = reader["report_date"];
            list.Add(new DashboardChartDataDto(
                reportDate is DateOnly dateOnly
                    ? dateOnly.ToDateTime(TimeOnly.MinValue)
                    : Convert.ToDateTime(reportDate),
                Convert.ToInt32(reader["total_bills"]),
                Convert.ToDecimal(reader["daily_revenue"])
            ));
        }

        return list;
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(int limit = 5)
    {
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than 0.");

        var list = new List<TopProductDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetTopProducts, conn);
        cmd.Parameters.AddWithValue("limit", limit);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new TopProductDto(
                Convert.ToString(reader["product_name"]) ?? string.Empty,
                Convert.ToInt32(reader["total_sold"])
            ));
        }
        return list;
    }
}
