CREATE TABLE IF NOT EXISTS bills (
  id SERIAL PRIMARY KEY,
  buzzer_number INT NOT NULL,
  user_id INT REFERENCES users (id),
  total_amount DECIMAL(18, 0) DEFAULT 0,
  status bill_status DEFAULT 'pending' NOT NULL,
  cancel_reason VARCHAR(255) NULL,
  canceled_by INT REFERENCES users(id) NULL,
  created_at TIMESTAMPTZ DEFAULT NOW (),
  updated_at TIMESTAMPTZ DEFAULT NOW (),
  canceled_at TIMESTAMPTZ NULL
);
