using Npgsql;

namespace CoffeePOS.Data;

public static class DbInitializer
{
    public static void Initialize(string connStr)
    {
        using var conn = new NpgsqlConnection(connStr);
        conn.Open();

        string sqlUsers = @"
            CREATE TABLE IF NOT EXISTS users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(50) UNIQUE NOT NULL,
                password_hash VARCHAR(255) NOT NULL,
                full_name VARCHAR(100),
                role INT DEFAULT 1, -- 0: Admin, 1: Staff
                is_active BOOLEAN DEFAULT TRUE,
                created_at TIMESTAMP DEFAULT NOW(),
                updated_at TIMESTAMP DEFAULT NOW()
            );";
        ExecuteSql(conn, sqlUsers);

        string sqlCategories = @"
            CREATE TABLE IF NOT EXISTS categories (
                id SERIAL PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                is_deleted BOOLEAN DEFAULT FALSE
            );";
        ExecuteSql(conn, sqlCategories);

        string sqlProducts = @"
            CREATE TABLE IF NOT EXISTS products (
                id SERIAL PRIMARY KEY,
                category_id INT REFERENCES categories(id),
                name VARCHAR(200) NOT NULL,
                price DECIMAL(18,0) DEFAULT 0,
                image_url VARCHAR(255),
                is_deleted BOOLEAN DEFAULT FALSE
            );";
        ExecuteSql(conn, sqlProducts);
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_products_category ON products(category_id);");

        string sqlBills = @"
            CREATE TABLE IF NOT EXISTS bills (
                id SERIAL PRIMARY KEY,
                buzzer_number INT NOT NULL,
                user_id INT REFERENCES users(id),
                order_type INT DEFAULT 1,
                total_amount DECIMAL(18,0) DEFAULT 0,
                status INT DEFAULT 1, -- 0: Unpaid, 1: Paid
                is_deleted BOOLEAN DEFAULT FALSE,
                created_at TIMESTAMP DEFAULT NOW(),
                updated_at TIMESTAMP DEFAULT NOW()
            );";
        ExecuteSql(conn, sqlBills);
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_bills_userid ON bills(user_id);");
        ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_bills_reporting ON bills(created_at) WHERE is_deleted = false;");

        string sqlBillDetails = @"
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

        string sqlShiftReports = @"
            CREATE TABLE IF NOT EXISTS shift_reports (
                id SERIAL PRIMARY KEY,
                user_id INT REFERENCES users(id),

                start_time TIMESTAMP NOT NULL,
                end_time TIMESTAMP DEFAULT NOW(),

                total_bills INT DEFAULT 0,            -- Tổng số lượng đơn đã bán trong ca
                expected_cash DECIMAL(18,0) NOT NULL, -- Số tiền hệ thống cộng lại từ bảng bills
                actual_cash DECIMAL(18,0) NOT NULL,   -- Số tiền thu ngân đếm thực tế nhập vào
                variance DECIMAL(18,0) NOT NULL,      -- Độ lệch (actual_cash - expected_cash)

                note VARCHAR(255),                -- Lời giải trình nếu tiền bị lệch
                created_at TIMESTAMP DEFAULT NOW()
            );";
        ExecuteSql(conn, sqlShiftReports);
        ExecuteSql(conn, @"
            CREATE INDEX IF NOT EXISTS idx_shiftreports_userid ON shift_reports(user_id);
            CREATE INDEX IF NOT EXISTS idx_shiftreports_created_at ON shift_reports(created_at);
            CREATE INDEX IF NOT EXISTS idx_shiftreports_start_time ON shift_reports(start_time);
            CREATE INDEX IF NOT EXISTS idx_shiftreports_end_time ON shift_reports(end_time);
        ");

        SeedAdminUser(conn);
    }

    private static void ExecuteSql(NpgsqlConnection conn, string sql)
    {
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static void SeedAdminUser(NpgsqlConnection conn)
    {
        string checkSql = "SELECT COUNT(1) FROM users WHERE username = 'admin'";
        using var checkCmd = new NpgsqlCommand(checkSql, conn);
        long count = (long)checkCmd.ExecuteScalar()!;

        if (count == 0)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 11);

            string insertSql = @"
                INSERT INTO users (username, password_hash, full_name, role)
                VALUES ('admin', @hash, 'Administrator', 0)";
            using var insertCmd = new NpgsqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("hash", hash);
            insertCmd.ExecuteNonQuery();
        }
    }
}
