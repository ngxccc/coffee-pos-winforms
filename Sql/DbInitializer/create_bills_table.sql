CREATE TABLE IF NOT EXISTS bills (
  id SERIAL PRIMARY KEY,
  buzzer_number INT NOT NULL,
  user_id INT REFERENCES users (id),
  order_type INT DEFAULT 1,
  total_amount DECIMAL(18, 0) DEFAULT 0,
  status INT DEFAULT 1,
  is_deleted BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP DEFAULT NOW (),
  updated_at TIMESTAMP DEFAULT NOW (),
  deleted_at TIMESTAMP NULL
);
