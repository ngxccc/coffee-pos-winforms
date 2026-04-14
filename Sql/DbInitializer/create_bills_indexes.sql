CREATE INDEX IF NOT EXISTS idx_bills_userid ON bills(user_id);
CREATE INDEX IF NOT EXISTS idx_bills_reporting ON bills(created_at)
WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_bills_created_at_date ON bills (
  (
    (created_at AT TIME ZONE 'Asia/Ho_Chi_Minh')::date
  )
);
