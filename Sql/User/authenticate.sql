SELECT id,
  username,
  password_hash,
  full_name,
  role,
  is_active
FROM users
WHERE username = @u;
