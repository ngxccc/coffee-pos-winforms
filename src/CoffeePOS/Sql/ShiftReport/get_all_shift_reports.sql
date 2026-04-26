SELECT sr.id,
  u.full_name as cashier_name,
  sr.start_time,
  sr.end_time,
  sr.total_bills,
  sr.starting_cash,
  sr.expected_cash,
  sr.actual_cash,
  sr.difference,
  sr.note
FROM shift_reports sr
  JOIN users u ON sr.user_id = u.id
ORDER BY sr.end_time DESC;
