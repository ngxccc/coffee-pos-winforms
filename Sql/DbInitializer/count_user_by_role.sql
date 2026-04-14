SELECT COUNT(*)
FROM users
WHERE role = CAST(@role AS user_role);
