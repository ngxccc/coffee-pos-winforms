UPDATE products
SET name = @name,
    price = @price,
    category_id = @categoryId,
    image_url = @imageUrl,
    updated_at = NOW()
WHERE id = @id
  AND is_deleted = false;
