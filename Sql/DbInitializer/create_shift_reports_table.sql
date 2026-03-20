CREATE TABLE IF NOT EXISTS shift_reports (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(id),
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP DEFAULT NOW(),
    total_bills INT DEFAULT 0,
    expected_cash DECIMAL(18,0) NOT NULL,
    actual_cash DECIMAL(18,0) NOT NULL,
    variance DECIMAL(18,0) NOT NULL,
    note VARCHAR(255),
    created_at TIMESTAMP DEFAULT NOW()
);
