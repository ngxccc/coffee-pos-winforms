SELECT id, name, is_deleted, created_at, updated_at, deleted_at
FROM categories
WHERE id = @id
  AND is_deleted = true;
