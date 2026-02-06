using Npgsql;

namespace CoffeePOS.Data;

public static class DbInitializer
{
    public static void Initialize(string connStr)
    {
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();

        var sqlCategories = @"
            CREATE TABLE IF NOT EXISTS categories (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100) NOT NULL
            );";
        ExecuteSql(conn, sqlCategories);

        var sqlProducts = @"
            CREATE TABLE IF NOT EXISTS products (
                id SERIAL PRIMARY KEY,
                name VARCHAR(200) NOT NULL,
                price DECIMAL(18, 0) NOT NULL DEFAULT 0,
                category_id INT REFERENCES categories(id) ON DELETE SET NULL
            );";
        ExecuteSql(conn, sqlProducts);

        var sqlBills = @"
            CREATE TABLE IF NOT EXISTS bills (
                id SERIAL PRIMARY KEY,
                table_id INT NOT NULL,
                total_amount DECIMAL(18,0) DEFAULT 0,
                status INT DEFAULT 0, -- 0: Unpaid, 1: Paid
                created_at TIMESTAMP DEFAULT NOW(),
                checkout_at TIMESTAMP
            );";
        ExecuteSql(conn, sqlBills);

        if (CountTable(conn, "categories") == 0)
        {
            var seedSql = @"
                INSERT INTO categories (name) VALUES
                ('Cà phê'), ('Trà trái cây'), ('Đá xay'), ('Bánh ngọt');

                -- Insert vài món mẫu
                INSERT INTO products (name, price, category_id) VALUES
                ('Cafe Đen', 25000, 1),
                ('Cafe Sữa', 29000, 1),
                ('Trà Đào Cam Sả', 35000, 2),
                ('Bánh Tiramisu', 45000, 4);
            ";
            ExecuteSql(conn, seedSql);
        }
    }

    private static void ExecuteSql(NpgsqlConnection conn, string sql)
    {
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static long CountTable(NpgsqlConnection conn, string tableName)
    {
        using var cmd = new NpgsqlCommand($"SELECT COUNT(*) FROM {tableName}", conn);
        return (long)(cmd.ExecuteScalar() ?? 0);
    }
}
