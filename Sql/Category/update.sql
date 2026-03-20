UPDATE categories
SET name = @name,
    updated_at = NOW()
WHERE id = @id
  AND is_deleted = false;
