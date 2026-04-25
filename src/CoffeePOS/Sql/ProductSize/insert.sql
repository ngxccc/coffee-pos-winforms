INSERT INTO product_sizes (product_id, size_name, price_adjustment)
VALUES (
    @product_id,
    CAST(@size_name AS product_size),
    @price_adjustment
  )
RETURNING id;
