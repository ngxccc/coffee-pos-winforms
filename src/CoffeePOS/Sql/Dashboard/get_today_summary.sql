SELECT COALESCE(SUM(total_amount), 0) AS revenue,
  COUNT(*) AS order_count,
  COALESCE(AVG(total_amount), 0) AS avg_order
FROM bills
WHERE created_at::date = CURRENT_DATE
  AND status = 'paid';
