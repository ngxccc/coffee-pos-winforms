UPDATE bills
SET is_deleted = true,
  cancel_reason = @reason,
  deleted_at = NOW(),
  updated_at = NOW()
WHERE id = @id
  AND is_deleted = false;
