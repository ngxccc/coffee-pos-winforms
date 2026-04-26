INSERT INTO bills (buzzer_number, user_id, status, total_amount)
VALUES (@buzzer_number, @user_id, 'paid', @total_amount)
RETURNING id;
