SELECT TRIM(SPLIT_PART(bd.product_name, ' (', 1)) AS product_name,
  SUM(quantity) AS total_sold
FROM bill_details bd
  INNER JOIN bills b ON bd.bill_id = b.id
WHERE b.is_deleted = false
GROUP BY 1
ORDER BY total_sold DESC
LIMIT @limit;
