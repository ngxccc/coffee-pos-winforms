-- WHY: Tự động bỏ qua nếu username đã tồn tại trong DB, đảm bảo tính Idempotent cho Migration Script.
INSERT INTO users (username, password_hash, full_name, role)
VALUES -- Hash của 'admin123'
  (
    'admin',
    '$2a$11$ZBJeu8AFm4GPaQzRgycZuuBVJY1dJ3z8e33GK.v3hwlUUxA/5zTB2',
    'Seed Admin',
    'admin'
  ),
  -- Hash của '123123'
  (
    'cashier',
    '$2a$11$ldxWVaOOKQpv4NZcL96pcOnXMtMhCjnTR8DsXg6Dn2nw12ej9p3de',
    'Seed Cashier',
    'cashier'
  ) ON CONFLICT (username) DO NOTHING;
