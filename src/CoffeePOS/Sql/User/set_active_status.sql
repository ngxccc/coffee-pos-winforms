UPDATE users
SET is_active = @isActive,
  updated_at = NOW()
WHERE id = @id;
