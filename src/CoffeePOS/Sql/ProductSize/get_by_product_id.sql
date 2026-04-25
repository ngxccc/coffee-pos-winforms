SELECT ps.id,
  ps.product_id,
  p.name AS product_name,
  ps.size_name::text AS size_name,
  ps.price_adjustment
FROM product_sizes ps
  INNER JOIN products p ON ps.product_id = p.id
WHERE ps.product_id = @product_id
ORDER BY ps.price_adjustment ASC;
