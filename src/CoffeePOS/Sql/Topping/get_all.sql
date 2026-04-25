SELECT id,
  name,
  price,
  created_at,
  updated_at,
  deleted_at
FROM toppings
WHERE is_deleted = @is_deleted
ORDER BY CASE
    WHEN @is_deleted THEN deleted_at
  END DESC,
  CASE
    WHEN NOT @is_deleted THEN name
  END ASC;
