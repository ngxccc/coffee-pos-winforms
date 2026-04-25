INSERT INTO toppings (name, price)
VALUES (@name, @price)
RETURNING id;
