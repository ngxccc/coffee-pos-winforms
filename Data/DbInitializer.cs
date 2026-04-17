using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CoffeePOS.Data;

public static class DbInitializer
{
    private static readonly string SqlCreateEnums = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateEnums);
    private static readonly string SqlCreateUsersTable = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateUsersTable);
    private static readonly string SqlCreateCategoriesTable = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateCategoriesTable);
    private static readonly string SqlCreateProductsTable = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateProductsTable);
    private static readonly string SqlCreateProductsIndexes = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateProductsIndexes);
    private static readonly string SqlCreateBillsTable = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateBillsTable);
    private static readonly string SqlCreateBillsIndexes = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateBillsIndexes);
    private static readonly string SqlCreateBillDetailsTable = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateBillDetailsTable);
    private static readonly string SqlCreateToppingsTable = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateToppingsTable);
    private static readonly string SqlCreateBillDetailToppingsTable = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateBillDetailToppingsTable);
    private static readonly string SqlCreateShiftReportsTable = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateShiftReportsTable);
    private static readonly string SqlCreateShiftReportsIndexes = SqlFileLoader.Load(SqlKeys.DbInitializer.CreateShiftReportsIndexes);
    private static readonly string SqlCountUserByRole = SqlFileLoader.Load(SqlKeys.DbInitializer.CountUserByRole);
    private static readonly string SqlInsertSeedUser = SqlFileLoader.Load(SqlKeys.DbInitializer.InsertSeedUser);

    // PERF: Utilize NpgsqlDataSource for built-in connection pooling and multiplexing
    public static async Task InitializeAsync(NpgsqlDataSource dataSource, IConfiguration config)
    {
        // WHY: Short-circuit directly from configuration. Zero database hits required to check state.
        bool shouldRunMigration = config.GetValue<bool>("SystemConfig:RunDatabaseMigrationsOnStartup");

        if (!shouldRunMigration)
        {
            return;
        }

        await using var conn = await dataSource.OpenConnectionAsync();

        // TODO: Replace manual script execution with DbUp or EF Core Migrations for real version tracking.
        await ExecuteSqlAsync(conn, SqlCreateEnums);
        await ExecuteSqlAsync(conn, SqlCreateUsersTable);
        await ExecuteSqlAsync(conn, SqlCreateCategoriesTable);
        await ExecuteSqlAsync(conn, SqlCreateProductsTable);
        await ExecuteSqlAsync(conn, SqlCreateProductsIndexes);
        await ExecuteSqlAsync(conn, SqlCreateBillsTable);
        await ExecuteSqlAsync(conn, SqlCreateBillsIndexes);
        await ExecuteSqlAsync(conn, SqlCreateBillDetailsTable);
        await ExecuteSqlAsync(conn, SqlCreateToppingsTable);
        await ExecuteSqlAsync(conn, SqlCreateBillDetailToppingsTable);
        await ExecuteSqlAsync(conn, SqlCreateShiftReportsTable);
        await ExecuteSqlAsync(conn, SqlCreateShiftReportsIndexes);

        await SeedAdminUserAsync(conn);
    }

    private static async Task ExecuteSqlAsync(NpgsqlConnection conn, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task SeedAdminUserAsync(NpgsqlConnection conn)
    {
        long countAdmin = await GetUserCountByRoleAsync(conn, "admin");
        long countCashier = await GetUserCountByRoleAsync(conn, "cashier");

        if (countAdmin == 0)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 11);
            await InsertSeedUserAsync(conn, "admin", hash, "Seed Admin", "admin");
        }

        if (countCashier == 0)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("123123", workFactor: 11);
            await InsertSeedUserAsync(conn, "cashier", hash, "Seed Cashier", "cashier");
        }
    }

    private static async Task<long> GetUserCountByRoleAsync(NpgsqlConnection conn, string role)
    {
        await using var cmd = new NpgsqlCommand(SqlCountUserByRole, conn);
        cmd.Parameters.AddWithValue("role", role);
        var result = await cmd.ExecuteScalarAsync();
        return (long)(result ?? 0L);
    }

    private static async Task InsertSeedUserAsync(NpgsqlConnection conn, string username, string hash, string fullName, string role)
    {
        await using var cmd = new NpgsqlCommand(SqlInsertSeedUser, conn);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("hash", hash);
        cmd.Parameters.AddWithValue("fullName", fullName);
        cmd.Parameters.AddWithValue("role", role);
        await cmd.ExecuteNonQueryAsync();
    }
}
