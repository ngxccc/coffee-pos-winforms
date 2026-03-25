INSERT INTO categories (name)
VALUES ('Cà phê'),
  ('Trà trái cây'),
  ('Đá xay');
-- Insert vài món mẫu
INSERT INTO products (name, price, category_id)
VALUES ('Cafe Đen', 25000, 1),
  ('Cafe Sữa', 29000, 1),
  ('Trà Đào Cam Sả', 35000, 2);
DO $$
DECLARE i INT;
cat_id INT;
BEGIN FOR i IN 4..100 LOOP cat_id := floor(random() * 3 + 1)::int;
-- Random category 1-4
INSERT INTO products (name, price, category_id)
VALUES (
    'Món ngon số ' || i,
    (floor(random() * 50) + 10) * 1000,
    cat_id
  );
END LOOP;
END $$;
