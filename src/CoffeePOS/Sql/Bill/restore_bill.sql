UPDATE bills
SET status = 'paid'::bill_status,
  cancel_reason = NULL,
  canceled_at = NULL,
  canceled_by = NULL,
  updated_at = NOW()
WHERE id = @id
  AND status = 'canceled'::bill_status
