SELECT p.id,
  p.name,
  p.price,
  p.category_id,
  p.image_url,
  ps.size_name,
  ps.price_adjustment,
  p.is_deleted,
  p.created_at,
  p.updated_at,
  p.deleted_at
FROM products p
  LEFT JOIN product_sizes ps ON p.id = ps.product_id
WHERE p.is_deleted = true
ORDER BY p.id DESC,
  ps.price_adjustment ASC;
