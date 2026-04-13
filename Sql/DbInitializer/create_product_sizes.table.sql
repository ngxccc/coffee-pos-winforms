CREATE TABLE IF NOT EXISTS product_sizes (
  id SERIAL PRIMARY KEY,
  product_id INT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
  size_name product_size NOT NULL,
  price_adjustment DECIMAL(18, 2) NOT NULL DEFAULT 0,
  UNIQUE(product_id, size_name)
);
