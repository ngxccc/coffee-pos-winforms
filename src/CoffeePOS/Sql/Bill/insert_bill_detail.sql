INSERT INTO bill_details (
    bill_id,
    product_id,
    product_name,
    quantity,
    order_type,
    base_price,
    note
  )
VALUES (
    @bill_id,
    @product_id,
    @product_name,
    @quantity,
    @order_type,
    @base_price,
    @note
  );
