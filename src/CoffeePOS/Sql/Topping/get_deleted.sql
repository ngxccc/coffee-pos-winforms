SELECT id,
  name,
  price,
  deleted_at
FROM toppings
WHERE is_deleted = TRUE
ORDER BY deleted_at DESC;
