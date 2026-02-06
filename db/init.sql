CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    price DECIMAL(18, 0) NOT NULL DEFAULT 0,
    category_id INT REFERENCES categories(id) ON DELETE SET NULL
);

INSERT INTO categories (name) VALUES
('Cà phê'), ('Trà trái cây'), ('Đá xay'), ('Bánh ngọt');

DO $$
DECLARE
    i INT;
    cat_id INT;
BEGIN
    FOR i IN 1..1000 LOOP
        cat_id := floor(random() * 4 + 1)::int; -- Random category 1-4
        INSERT INTO products (name, price, category_id)
        VALUES ('Món ngon số ' || i, (floor(random() * 50) + 10) * 1000, cat_id);
    END LOOP;
END $$;
