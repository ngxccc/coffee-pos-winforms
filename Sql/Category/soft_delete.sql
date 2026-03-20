UPDATE categories
SET is_deleted = true,
    updated_at = NOW(),
    deleted_at = NOW()
WHERE id = @id
  AND is_deleted = false;
