SELECT b.id,
  b.buzzer_number,
  b.total_amount,
  b.created_at,
  b.is_deleted,
  b.deleted_at,
  COALESCE(u.full_name, 'N/A') AS created_by_name
FROM bills b
  LEFT JOIN users u ON u.id = b.user_id
WHERE b.created_at >= @from_date
  AND b.created_at < @to_date
ORDER BY b.created_at DESC;
