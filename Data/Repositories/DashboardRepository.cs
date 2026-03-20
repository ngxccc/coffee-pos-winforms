using CoffeePOS.Shared.Dtos;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class DashboardRepository(NpgsqlDataSource dataSource) : IDashboardRepository
{
    public async Task<TodaySummaryDto> GetTodaySummaryAsync()
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = @"
            SELECT
                COALESCE(SUM(total_amount), 0) AS revenue,
                COUNT(*) AS order_count,
                COALESCE(AVG(total_amount), 0) AS avg_order
            FROM bills
            WHERE created_at::date = CURRENT_DATE
              AND is_deleted = FALSE
              AND status = 1;";

        using var cmd = new NpgsqlCommand(sql, conn);
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
        var list = new List<DashboardChartDataDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"
            WITH date_range AS (
                SELECT generate_series(
                    CURRENT_DATE - ((@days - 1) * INTERVAL '1 day'),
                    CURRENT_DATE,
                    INTERVAL '1 day'
                )::date AS report_date
            )
            SELECT
                dr.report_date,
                COUNT(b.id) AS total_bills,
                COALESCE(SUM(b.total_amount), 0) AS daily_revenue
            FROM date_range dr
            LEFT JOIN bills b ON dr.report_date = b.created_at::date
                AND b.is_deleted = FALSE
                AND b.status = 1
                AND b.created_at >= (CURRENT_DATE - ((@days - 1) * INTERVAL '1 day'))
            GROUP BY dr.report_date
            ORDER BY dr.report_date ASC;";

        using var cmd = new NpgsqlCommand(sql, conn);
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
        var list = new List<TopProductDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"
        SELECT product_name, SUM(quantity) as total_sold
        FROM bill_details bd
        JOIN bills b ON bd.bill_id = b.id
            AND b.status = 1
            AND b.is_deleted = false
        GROUP BY product_name
        ORDER BY total_sold DESC
        LIMIT @limit;";

        using var cmd = new NpgsqlCommand(sql, conn);
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
