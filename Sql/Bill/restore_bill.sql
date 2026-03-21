UPDATE bills
SET is_deleted = false,
  deleted_at = NULL,
  updated_at = NOW()
WHERE id = @id
  AND is_deleted = true;
