SELECT b.id,
  b.buzzer_number,
  COALESCE(SUM(bd.quantity), 0)::int AS total_items,
  b.total_amount,
  b.status,
  b.canceled_at,
  b.created_at
FROM bills b
  LEFT JOIN bill_details bd ON b.id = bd.bill_id
WHERE b.user_id = @uid
  AND b.created_at >= CURRENT_DATE
GROUP BY b.id,
  b.buzzer_number,
  b.total_amount,
  b.created_at
ORDER BY b.created_at DESC;
