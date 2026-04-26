INSERT INTO shift_reports (
    user_id,
    start_time,
    end_time,
    total_bills,
    starting_cash,
    expected_cash,
    actual_cash,
    difference,
    note
  )
VALUES (
    @user_id,
    @start_time,
    @end_time,
    @total_bills,
    @starting_cash,
    @expected_cash,
    @actual_cash,
    @difference,
    @note
  )
RETURNING id;
