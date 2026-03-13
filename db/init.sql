CREATE TABLE categories (
  id SERIAL PRIMARY KEY,
  name VARCHAR(100) NOT NULL
);

CREATE TABLE products (
  id SERIAL PRIMARY KEY,
  name VARCHAR(200) NOT NULL,
  price DECIMAL(18, 0) NOT NULL DEFAULT 0,
  category_id INT REFERENCES categories(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS bills (
  id SERIAL PRIMARY KEY,
  buzzer_number INT NOT NULL,
  order_type INT DEFAULT 1,
  total_amount DECIMAL(18,0) DEFAULT 0,
  status INT DEFAULT 1, -- 0: Unpaid, 1: Paid
  is_deleted BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

-- 0: Queued (Đang chờ pha)
-- 1: Preparing (Đang pha chế)
-- 2: Ready (Đã xong, chờ khách lấy)
-- 3: Delivered (Đã giao khách)

CREATE TABLE IF NOT EXISTS bill_details (
  id SERIAL PRIMARY KEY,
  bill_id INT NOT NULL,
  product_id INT NOT NULL,
  product_name VARCHAR(200),
  item_status INT DEFAULT 0,
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

INSERT INTO categories (name) VALUES
('Cà phê'), ('Trà trái cây'), ('Đá xay');

DO $$
DECLARE
  i INT;
  cat_id INT;
BEGIN
  FOR i IN 1..1000 LOOP
    cat_id := floor(random() * 4 + 1)::int; -- Random category 1-4
    INSERT INTO products (name, price, category_id)
    VALUES ('Món ngon số ' || i, (floor(random() * 50) + 10) * 1000, cat_id);
  END LOOP;
END $$;
