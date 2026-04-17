UPDATE users
SET is_active = false,
    updated_at = NOW()
WHERE id = @id;
