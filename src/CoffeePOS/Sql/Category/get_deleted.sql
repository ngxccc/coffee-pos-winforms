SELECT id, name, is_deleted, created_at, updated_at, deleted_at
FROM categories
WHERE is_deleted = true
ORDER BY id;
