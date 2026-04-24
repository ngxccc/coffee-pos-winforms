UPDATE users
SET password_hash = @hash,
    updated_at = NOW()
WHERE id = @id;
