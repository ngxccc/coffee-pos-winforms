CREATE TABLE
  IF NOT EXISTS bill_details (
    id SERIAL PRIMARY KEY,
    bill_id INT NOT NULL,
    product_id INT NOT NULL,
    product_name VARCHAR(200),
    quantity INT DEFAULT 1,
    price DECIMAL(18, 0) DEFAULT 0,
    note VARCHAR(255),
    created_at TIMESTAMP DEFAULT NOW (),
    CONSTRAINT fk_bill_detail_bill FOREIGN KEY (bill_id) REFERENCES bills (id) ON DELETE CASCADE,
    CONSTRAINT fk_bill_detail_product FOREIGN KEY (product_id) REFERENCES products (id) ON DELETE CASCADE,
    UNIQUE (bill_id, product_id)
  );
