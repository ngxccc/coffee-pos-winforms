INSERT INTO users (
    username,
    password_hash,
    full_name,
    role
  )
VALUES (
    @username,
    @hash,
    @fullName,
    @role
  );
