SELECT product_id, product_name, quantity, price, note
FROM bill_details
WHERE bill_id = @b
ORDER BY id;
