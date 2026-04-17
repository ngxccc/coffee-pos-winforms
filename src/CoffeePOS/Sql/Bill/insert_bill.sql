INSERT INTO bills (buzzer_number, user_id, status, total_amount)
VALUES (@b, @u, 'paid', @total)
RETURNING id;
