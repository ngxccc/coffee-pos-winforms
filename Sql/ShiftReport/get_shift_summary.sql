SELECT
    COUNT(id) AS total_bills,
    COALESCE(SUM(total_amount), 0) AS expected_cash
FROM bills
WHERE user_id = @uid
  AND created_at >= @start
  AND created_at <= @end
  AND is_deleted = false;
