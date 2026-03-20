SELECT id, name, is_deleted, created_at, updated_at, deleted_at
FROM categories
WHERE is_deleted = false
ORDER BY id;
