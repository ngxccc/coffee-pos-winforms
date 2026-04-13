CREATE TABLE IF NOT EXISTS bills (
  id SERIAL PRIMARY KEY,
  buzzer_number INT NOT NULL,
  user_id INT REFERENCES users (id),
  order_type bill_order_type DEFAULT 'dine_in' NOT NULL,
  total_amount DECIMAL(18, 0) DEFAULT 0,
  status bill_status DEFAULT 'pending' NOT NULL,
  cancel_reason VARCHAR(255) NULL,
  is_deleted BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP DEFAULT NOW (),
  updated_at TIMESTAMP DEFAULT NOW (),
  deleted_at TIMESTAMP NULL
);
