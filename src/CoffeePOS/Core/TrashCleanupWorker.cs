using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;

namespace CoffeePOS.Core;

public class TrashCleanupWorker(NpgsqlDataSource dataSource) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var conn = await dataSource.OpenConnectionAsync(stoppingToken);

                string sql = @"
                    DELETE FROM products WHERE is_deleted = true AND deleted_at < NOW() - INTERVAL '30 days';
                    DELETE FROM categories WHERE is_deleted = true AND deleted_at < NOW() - INTERVAL '30 days';";

                using var cmd = new NpgsqlCommand(sql, conn);
                int rowsDeleted = await cmd.ExecuteNonQueryAsync(stoppingToken);

                if (rowsDeleted > 0)
                {
                    Log.Information($"[Lao Công Dọn Rác]: Đã dọn vĩnh viễn {rowsDeleted} bản ghi hết hạn!");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Lỗi Lao Công]: {ex.Message}");
            }

            _ = Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
