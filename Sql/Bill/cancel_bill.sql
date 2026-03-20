UPDATE bills
SET is_deleted = true,
    deleted_at = NOW(),
    updated_at = NOW()
WHERE id = @id
  AND is_deleted = false;
