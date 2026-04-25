UPDATE toppings
SET is_deleted = TRUE,
  deleted_at = NOW()
WHERE id = @id;
