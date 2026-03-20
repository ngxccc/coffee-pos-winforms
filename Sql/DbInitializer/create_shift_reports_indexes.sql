CREATE INDEX IF NOT EXISTS idx_shiftreports_userid ON shift_reports(user_id);
CREATE INDEX IF NOT EXISTS idx_shiftreports_created_at ON shift_reports(created_at);
CREATE INDEX IF NOT EXISTS idx_shiftreports_start_time ON shift_reports(start_time);
CREATE INDEX IF NOT EXISTS idx_shiftreports_end_time ON shift_reports(end_time);
