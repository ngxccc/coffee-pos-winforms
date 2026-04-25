UPDATE toppings
SET name = @name,
  price = @price,
  updated_at = NOW()
WHERE id = @id
  AND is_deleted = FALSE;
