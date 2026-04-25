INSERT INTO users (
    username,
    password_hash,
    full_name,
    role
  )
VALUES (
    @username,
    @hash,
    @full_name,
    CAST(@role AS user_role)
  );
