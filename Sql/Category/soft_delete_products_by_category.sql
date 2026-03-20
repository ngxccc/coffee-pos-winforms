UPDATE products
SET is_deleted = true,
    updated_at = NOW(),
    deleted_at = NOW()
WHERE category_id = @id
  AND is_deleted = false;
