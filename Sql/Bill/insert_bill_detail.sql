INSERT INTO bill_details (
    bill_id,
    product_id,
    product_name,
    quantity,
    base_price,
    note
  )
VALUES (@b, @p, @n, @q, @basePrice, @note);
