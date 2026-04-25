using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos.Dashboard;
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
            decimal revenue = reader.GetRequired<decimal>("revenue");
            int count = reader.GetRequired<int>("order_count");
            decimal avgOrder = reader.GetRequired<decimal>("avg_order");

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
        cmd.Parameters.Add(new NpgsqlParameter<int>("days", days));
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new DashboardChartDataDto(
                reader.GetRequired<DateTime>("report_date"),
                reader.GetRequired<int>("total_bills"),
                reader.GetRequired<decimal>("daily_revenue")
            ));
        }

        return list;
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(int limit = 5)
    {
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than 0.");

        var list = new List<TopProductDto>();
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(SqlGetTopProducts, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("limit", limit));

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new TopProductDto(
                reader.GetRequired<string>("product_name"),
                reader.GetRequired<int>("total_sold")
            ));
        }
        return list;
    }
}
