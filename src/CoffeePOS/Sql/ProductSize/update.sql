UPDATE product_sizes
SET price_adjustment = @price_adjustment,
  updated_at = NOW()
WHERE id = @id
  AND product_id = @product_id;
