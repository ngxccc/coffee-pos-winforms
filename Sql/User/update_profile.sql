UPDATE users
SET username = @username,
  full_name = @fullName,
  role = @role,
  updated_at = NOW()
WHERE id = @id;
