INSERT INTO shift_reports
(user_id, start_time, end_time, total_bills, expected_cash, actual_cash, variance, note)
VALUES (@u, @start, @end, @bills, @expected, @actual, @variance, @note);
