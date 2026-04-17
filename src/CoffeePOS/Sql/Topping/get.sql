SELECT id,
  name,
  price
FROM toppings
WHERE is_deleted = @is_deleted
ORDER BY name ASC;
