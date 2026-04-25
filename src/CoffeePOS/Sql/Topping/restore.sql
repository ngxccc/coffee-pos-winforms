UPDATE toppings
SET is_deleted = FALSE,
  deleted_at = NULL,
  updated_at = NOW()
WHERE id = @id;
