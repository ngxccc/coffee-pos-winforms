drop table bills, categories, products, bill_details, shift_reports cascade;

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    role INT DEFAULT 1, -- 0: Admin, 1: Staff
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    is_deleted BOOLEAN DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS products (
    id SERIAL PRIMARY KEY,
    category_id INT REFERENCES categories(id),
    name VARCHAR(200) NOT NULL,
    price DECIMAL(18,0) DEFAULT 0,
    image_url VARCHAR(255),
    is_deleted BOOLEAN DEFAULT FALSE
);
CREATE INDEX IF NOT EXISTS idx_products_category ON products(category_id);

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
);
CREATE INDEX IF NOT EXISTS idx_bills_userid ON bills(user_id);
CREATE INDEX IF NOT EXISTS idx_bills_reporting ON bills(created_at) WHERE is_deleted = false;

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
);

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
);
CREATE INDEX IF NOT EXISTS idx_shiftreports_userid ON shift_reports(user_id);
CREATE INDEX IF NOT EXISTS idx_shiftreports_created_at ON shift_reports(created_at);
CREATE INDEX IF NOT EXISTS idx_shiftreports_start_time ON shift_reports(start_time);
CREATE INDEX IF NOT EXISTS idx_shiftreports_end_time ON shift_reports(end_time);

-- Dữ liệu mẫu
INSERT INTO categories (name) VALUES
('Cà phê'), ('Trà trái cây'), ('Đá xay');

-- Insert vài món mẫu
INSERT INTO products (name, price, category_id) VALUES
('Cafe Đen', 25000, 1),
('Cafe Sữa', 29000, 1),
('Trà Đào Cam Sả', 35000, 2);

DO $$
DECLARE
    i INT;
    cat_id INT;
BEGIN
    FOR i IN 4..1000 LOOP
        cat_id := floor(random() * 3 + 1)::int; -- Random category 1-4
        INSERT INTO products (name, price, category_id)
        VALUES ('Món ngon số ' || i, (floor(random() * 50) + 10) * 1000, cat_id);
    END LOOP;
END $$;

EXPLAIN ANALYZE
WITH date_range AS (
    SELECT generate_series(
        CURRENT_DATE - INTERVAL '6 days',
        CURRENT_DATE,
        '1 day'
    )::date AS report_date
)
SELECT
    dr.report_date,
    COUNT(b.id) AS total_bills,
    COALESCE(SUM(b.total_amount), 0) AS daily_revenue
FROM date_range dr
LEFT JOIN bills b ON dr.report_date = b.created_at::date
    AND b.is_deleted = FALSE
    AND b.status = 1 -- Chỉ tính các hóa đơn đã thanh toán
GROUP BY dr.report_date
ORDER BY dr.report_date ASC;
