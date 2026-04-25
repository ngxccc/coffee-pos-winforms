DO $$
DECLARE sql_cmd text;
BEGIN FOREACH sql_cmd IN ARRAY [
        $cmd$ CREATE TYPE product_size AS ENUM ('S', 'M', 'L', 'XL') $cmd$,
        $cmd$ CREATE TYPE bill_order_type AS ENUM ('dine_in', 'takeaway') $cmd$,
        $cmd$ CREATE TYPE bill_status AS ENUM ('pending', 'paid', 'canceled') $cmd$,
        $cmd$ CREATE TYPE user_role AS ENUM ('admin', 'cashier') $cmd$
    ] LOOP BEGIN EXECUTE sql_cmd;
EXCEPTION
WHEN duplicate_object THEN null;
END;
END LOOP;
END $$;
