using CoffeePOS.Shared.Helpers;
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

    public static void Initialize(string connStr)
    {
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();

        ExecuteSql(conn, SqlCreateEnums);
        ExecuteSql(conn, SqlCreateUsersTable);
        ExecuteSql(conn, SqlCreateCategoriesTable);
        ExecuteSql(conn, SqlCreateProductsTable);
        ExecuteSql(conn, SqlCreateProductsIndexes);
        ExecuteSql(conn, SqlCreateBillsTable);
        ExecuteSql(conn, SqlCreateBillsIndexes);
        ExecuteSql(conn, SqlCreateBillDetailsTable);
        ExecuteSql(conn, SqlCreateToppingsTable);
        ExecuteSql(conn, SqlCreateBillDetailToppingsTable);
        ExecuteSql(conn, SqlCreateShiftReportsTable);
        ExecuteSql(conn, SqlCreateShiftReportsIndexes);

        SeedAdminUser(conn);
    }

    private static void ExecuteSql(NpgsqlConnection conn, string sql)
    {
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void SeedAdminUser(NpgsqlConnection conn)
    {
        long countAdmin = GetUserCountByRole(conn, "admin");
        long countCashier = GetUserCountByRole(conn, "cashier");

        if (countAdmin == 0)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 11);
            InsertSeedUser(conn, "admin", hash, "Seed Admin", "admin");
        }

        if (countCashier == 0)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("123123", workFactor: 11);
            InsertSeedUser(conn, "cashier", hash, "Seed Cashier", "cashier");
        }
    }

    private static long GetUserCountByRole(NpgsqlConnection conn, string role)
    {
        using var cmd = new NpgsqlCommand(SqlCountUserByRole, conn);
        cmd.Parameters.AddWithValue("role", role);
        return (long)cmd.ExecuteScalar()!;
    }

    private static void InsertSeedUser(NpgsqlConnection conn, string username, string hash, string fullName, string role)
    {
        using var cmd = new NpgsqlCommand(SqlInsertSeedUser, conn);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("hash", hash);
        cmd.Parameters.AddWithValue("fullName", fullName);
        cmd.Parameters.AddWithValue("role", role);
        cmd.ExecuteNonQuery();
    }
}
