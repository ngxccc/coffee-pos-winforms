using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories.Impl;

public class DashboardRepository(NpgsqlDataSource dataSource) : IDashboardRepository
{
    public async Task<List<DailyRevenue>> Get7DaysRevenueAsync()
    {
        var list = new List<DailyRevenue>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"
            WITH date_range AS (
                SELECT generate_series(CURRENT_DATE - INTERVAL '6 days', CURRENT_DATE, '1 day')::date AS report_date
            )
            SELECT
                dr.report_date,
                COUNT(b.id) AS total_bills,
                COALESCE(SUM(b.total_amount), 0) AS daily_revenue
            FROM date_range dr
            LEFT JOIN bills b ON dr.report_date = b.created_at::date
                AND b.is_deleted = FALSE
                AND b.status = 1
                AND b.created_at >= (CURRENT_DATE - INTERVAL '6 days')
            GROUP BY dr.report_date
            ORDER BY dr.report_date ASC;";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new DailyRevenue
            {
                Date = reader.GetDateTime(0),
                TotalBills = reader.GetInt32(1),
                Revenue = reader.GetDecimal(2)
            });
        }

        return list;
    }
}
