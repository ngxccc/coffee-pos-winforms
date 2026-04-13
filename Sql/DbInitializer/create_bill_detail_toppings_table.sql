CREATE TABLE IF NOT EXISTS bill_detail_toppings (
  id SERIAL PRIMARY KEY,
  bill_detail_id INT NOT NULL REFERENCES bill_details(id) ON DELETE CASCADE,
  topping_id INT NULL REFERENCES toppings(id) ON DELETE
  SET NULL,
    topping_name VARCHAR(100) NOT NULL,
    price DECIMAL(18, 2) NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT NOW()
);
