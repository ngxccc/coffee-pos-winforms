SELECT
    id, name, price, category_id, image_url,
    is_deleted, created_at, updated_at, deleted_at
FROM products
WHERE id = @id
  AND is_deleted = true;
