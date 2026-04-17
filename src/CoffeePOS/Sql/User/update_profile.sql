UPDATE users
SET username = @username,
  full_name = @fullName,
  role = CAST(@role AS user_role),
  updated_at = NOW()
WHERE id = @id;
