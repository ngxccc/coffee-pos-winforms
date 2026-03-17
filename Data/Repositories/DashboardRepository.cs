using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class DashboardRepository(NpgsqlDataSource dataSource) : IDashboardRepository
{
    public async Task<decimal> GetTodayRevenueAsync()
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = @"
            SELECT COALESCE(SUM(total_amount), 0)
            FROM bills
            WHERE created_at::date = CURRENT_DATE
              AND is_deleted = FALSE
              AND status = 1;";

        using var cmd = new NpgsqlCommand(sql, conn);
        var result = await cmd.ExecuteScalarAsync();

        return result is decimal revenue ? revenue : 0m;
    }

    public async Task<int> GetTodayOrderCountAsync()
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = @"
            SELECT COUNT(*)
            FROM bills
            WHERE created_at::date = CURRENT_DATE
              AND is_deleted = FALSE
              AND status = 1;";

        using var cmd = new NpgsqlCommand(sql, conn);
        var result = await cmd.ExecuteScalarAsync();

        return result is long count ? (int)count : 0;
    }

    public async Task<decimal> GetTodayAverageOrderAsync()
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = @"
            SELECT COALESCE(AVG(total_amount), 0)
            FROM bills
            WHERE created_at::date = CURRENT_DATE
              AND is_deleted = FALSE
              AND status = 1;";

        using var cmd = new NpgsqlCommand(sql, conn);
        var result = await cmd.ExecuteScalarAsync();

        return result is decimal avgOrder ? avgOrder : 0m;
    }

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

    public async Task<List<TopProduct>> GetTop5ProductsAsync()
    {
        var list = new List<TopProduct>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"
        SELECT product_name, SUM(quantity) as total_sold
        FROM bill_details bd
        JOIN bills b ON bd.bill_id = b.id
            AND b.status = 1
            AND b.is_deleted = false
        GROUP BY product_name
        ORDER BY total_sold DESC
        LIMIT 5;";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new TopProduct { ProductName = reader.GetString(0), TotalSold = reader.GetInt32(1) });
        }
        return list;
    }
}
