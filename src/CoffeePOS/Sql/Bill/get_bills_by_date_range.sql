SELECT b.id,
  b.buzzer_number,
  b.total_amount,
  b.status,
  b.created_at,
  b.canceled_at,
  COALESCE(u_creator.full_name, 'N/A') AS created_by_name,
  COALESCE(u_canceler.full_name, 'N/A') AS canceled_by_name
FROM bills b
  LEFT JOIN users u_creator ON u_creator.id = b.user_id
  LEFT JOIN users u_canceler ON u_canceler.id = b.canceled_by
WHERE b.created_at >= @from_date
  AND b.created_at < @to_date
ORDER BY b.created_at DESC;
