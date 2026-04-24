UPDATE products
SET is_deleted = false,
    updated_at = NOW(),
    deleted_at = NULL
WHERE id = @id
  AND is_deleted = true;
