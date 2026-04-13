CREATE TYPE IF NOT EXISTS product_size as ENUM ('S', 'M', 'L');
CREATE TYPE IF NOT EXISTS user_role as ENUM ('admin', 'cashier');
CREATE TYPE IF NOT EXISTS bill_order_type as ENUM ('dine_in', 'takeaway');
CREATE TYPE IF NOT EXISTS bill_status as ENUM ('pending', 'paid', 'canceled');
