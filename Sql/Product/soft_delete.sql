UPDATE products
SET is_deleted = true,
    updated_at = NOW(),
    deleted_at = NOW()
WHERE id = @id;
