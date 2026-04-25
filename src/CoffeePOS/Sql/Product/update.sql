UPDATE products
SET name = @name,
  price = @price,
  category_id = @category_id,
  image_url = @image_url,
  updated_at = NOW()
WHERE id = @id
  AND is_deleted = false;
