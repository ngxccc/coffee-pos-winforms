using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data;

public static class DbInitializer
{
    private static readonly string SqlCreateUsersTable = SqlFileLoader.Load("DbInitializer.create_users_table.sql");
    private static readonly string SqlCreateCategoriesTable = SqlFileLoader.Load("DbInitializer.create_categories_table.sql");
    private static readonly string SqlCreateProductsTable = SqlFileLoader.Load("DbInitializer.create_products_table.sql");
    private static readonly string SqlCreateProductsIndexes = SqlFileLoader.Load("DbInitializer.create_products_indexes.sql");
    private static readonly string SqlCreateBillsTable = SqlFileLoader.Load("DbInitializer.create_bills_table.sql");
    private static readonly string SqlCreateBillsIndexes = SqlFileLoader.Load("DbInitializer.create_bills_indexes.sql");
    private static readonly string SqlCreateBillDetailsTable = SqlFileLoader.Load("DbInitializer.create_bill_details_table.sql");
    private static readonly string SqlCreateShiftReportsTable = SqlFileLoader.Load("DbInitializer.create_shift_reports_table.sql");
    private static readonly string SqlCreateShiftReportsIndexes = SqlFileLoader.Load("DbInitializer.create_shift_reports_indexes.sql");
    private static readonly string SqlCountUserByUsername = SqlFileLoader.Load("DbInitializer.count_user_by_username.sql");
    private static readonly string SqlInsertSeedUser = SqlFileLoader.Load("DbInitializer.insert_seed_user.sql");

    public static void Initialize(string connStr)
    {
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();

        ExecuteSql(conn, SqlCreateUsersTable);
        ExecuteSql(conn, SqlCreateCategoriesTable);
        ExecuteSql(conn, SqlCreateProductsTable);
        ExecuteSql(conn, SqlCreateProductsIndexes);
        ExecuteSql(conn, SqlCreateBillsTable);
        ExecuteSql(conn, SqlCreateBillsIndexes);
        ExecuteSql(conn, SqlCreateBillDetailsTable);
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
        long countAdmin = GetUserCountByUsername(conn, "admin");
        long countEmployee = GetUserCountByUsername(conn, "staff");

        if (countAdmin == 0)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 11);
            InsertSeedUser(conn, "admin", hash, "Administrator", 0);
        }

        if (countEmployee == 0)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("123123", workFactor: 11);
            InsertSeedUser(conn, "staff", hash, "Staff", 1);
        }
    }

    private static long GetUserCountByUsername(NpgsqlConnection conn, string username)
    {
        using var cmd = new NpgsqlCommand(SqlCountUserByUsername, conn);
        cmd.Parameters.AddWithValue("username", username);
        return (long)cmd.ExecuteScalar()!;
    }

    private static void InsertSeedUser(NpgsqlConnection conn, string username, string hash, string fullName, int role)
    {
        using var cmd = new NpgsqlCommand(SqlInsertSeedUser, conn);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("hash", hash);
        cmd.Parameters.AddWithValue("fullName", fullName);
        cmd.Parameters.AddWithValue("role", role);
        cmd.ExecuteNonQuery();
    }
}
