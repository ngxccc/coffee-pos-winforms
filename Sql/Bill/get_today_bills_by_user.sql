SELECT id, buzzer_number, total_amount, created_at
FROM bills
WHERE user_id = @uid
  AND created_at >= CURRENT_DATE
  AND is_deleted = false
ORDER BY created_at DESC;
