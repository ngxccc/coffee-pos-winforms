using Microsoft.Extensions.Hosting;
using Npgsql;

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
                    Console.WriteLine($"[Lao Công Dọn Rác]: Đã dọn vĩnh viễn {rowsDeleted} bản ghi hết hạn!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi Lao Công]: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
