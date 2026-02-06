using Npgsql;

namespace CoffeePOS.Data;

public static class DbInitializer
{
    public static void Initialize(string connStr)
    {
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();

        var sqlTables = @"
            CREATE TABLE IF NOT EXISTS tables (
                id SERIAL PRIMARY KEY,
                name VARCHAR(50) NOT NULL
            );";
        ExecuteSql(conn, sqlTables);

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
                category_id INT,

                constraint fk_product_category
                    foreign key (category_id)
                    references categories(id)
                    on delete set null
            );";
        ExecuteSql(conn, sqlProducts);

        var sqlBills = @"
            CREATE TABLE IF NOT EXISTS bills (
                id SERIAL PRIMARY KEY,
                table_id INT,
                total_amount DECIMAL(18,0) DEFAULT 0,
                status INT DEFAULT 0, -- 0: Unpaid, 1: Paid
                created_at TIMESTAMP DEFAULT NOW(),
                checkout_at TIMESTAMP,

                constraint fk_bill_table
                    foreign key (table_id)
                    references tables(id)
                    on delete set null
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

        var sqlBillDetails = @"
            CREATE TABLE IF NOT EXISTS bill_details (
                id SERIAL PRIMARY KEY,
                bill_id INT NOT NULL,
                product_id INT NOT NULL,
                product_name VARCHAR(200),
                quantity INT DEFAULT 1,
                price DECIMAL(18,0) DEFAULT 0,
                note VARCHAR(255),
                created_at TIMESTAMP DEFAULT NOW(),

                constraint fk_bill_detail_bill
                    foreign key (bill_id)
                    references bills(id)
                    on delete cascade,
                constraint fk_bill_detail_product
                    foreign key (product_id)
                    references products(id)
                    on delete cascade,
                UNIQUE(bill_id, product_id)
            );";
        ExecuteSql(conn, sqlBillDetails);

        if (CountTable(conn, "tables") == 0)
        {
            // Seed 100 bàn
            for (int i = 1; i <= 99; i++)
            {
                ExecuteSql(conn, $"INSERT INTO tables (id, name) VALUES ({i}, 'Bàn {i:00}')");
            }
            // Reset sequence để insert bàn tiếp theo không lỗi ID
            ExecuteSql(conn, "SELECT setval('tables_id_seq', 99, true);");
        }

        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_bills_table_status ON bills(table_id, status);");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_products_category ON products(category_id);");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_bills_created_at ON bills(created_at);");
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
