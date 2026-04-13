WITH date_range AS (
  SELECT generate_series(
      CURRENT_DATE - ((@days - 1) * INTERVAL '1 day'),
      CURRENT_DATE,
      INTERVAL '1 day'
    )::date AS report_date
)
SELECT dr.report_date,
  COUNT(b.id) AS total_bills,
  COALESCE(SUM(b.total_amount), 0) AS daily_revenue
FROM date_range dr
  LEFT JOIN bills b ON dr.report_date = b.created_at::date
  AND b.is_deleted = FALSE
  AND b.status = 'paid'
  AND b.created_at >= (CURRENT_DATE - ((@days - 1) * INTERVAL '1 day'))
GROUP BY dr.report_date
ORDER BY dr.report_date ASC;
