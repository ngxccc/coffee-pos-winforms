SELECT id,
  name,
  price,
  created_at,
  updated_at
FROM toppings
WHERE is_deleted = FALSE
ORDER BY name ASC;
