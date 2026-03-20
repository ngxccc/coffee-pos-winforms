SELECT id, username, password_hash, full_name, role
FROM users
WHERE username = @u
  AND is_active = true;
