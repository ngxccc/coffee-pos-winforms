UPDATE users
SET is_active = @is_active,
  updated_at = NOW()
WHERE id = @id;
