SELECT product_name,
  SUM(quantity) AS total_sold
FROM bill_details bd
  JOIN bills b ON bd.bill_id = b.id
  AND b.status = 'paid'
  AND b.is_deleted = false
GROUP BY product_name
ORDER BY total_sold DESC
LIMIT @limit;
