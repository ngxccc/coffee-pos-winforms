UPDATE bills
SET status = 'canceled',
  cancel_reason = @cancel_reason,
  canceled_at = NOW(),
  canceled_by = @canceled_by,
  updated_at = NOW()
WHERE id = @id
  AND status != 'canceled';
