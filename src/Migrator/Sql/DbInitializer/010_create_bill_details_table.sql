CREATE TABLE IF NOT EXISTS bill_details (
  id SERIAL PRIMARY KEY,
  bill_id INT NOT NULL,
  product_id INT,
  product_name VARCHAR(200),
  quantity INT DEFAULT 1,
  order_type bill_order_type DEFAULT 'dine_in' NOT NULL,
  base_price DECIMAL(18, 2) NOT NULL DEFAULT 0,
  size_name product_size DEFAULT 'M',
  note VARCHAR(255),
  created_at TIMESTAMPTZ DEFAULT NOW (),
  CONSTRAINT fk_bill_detail_bill FOREIGN KEY (bill_id) REFERENCES bills (id) ON DELETE CASCADE,
  CONSTRAINT fk_bill_detail_product FOREIGN KEY (product_id) REFERENCES products (id) ON DELETE
  SET NULL
);
CREATE INDEX IF NOT EXISTS idx_bill_details_bill_id ON bill_details (bill_id);
