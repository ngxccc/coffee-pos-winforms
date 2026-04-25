UPDATE users
SET username = @username,
  full_name = @full_name,
  role = CAST(@role AS user_role),
  updated_at = NOW()
WHERE id = @id;
