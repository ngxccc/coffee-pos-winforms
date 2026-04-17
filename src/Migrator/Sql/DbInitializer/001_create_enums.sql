-- PERF: Single transaction block handling all enum initializations dynamically
-- WHY: Prevents Schema Pollution (no permanent functions created) and maintains strict DRY principles
DO $$
DECLARE sql_cmd text;
BEGIN -- HACK: Using specific dollar tags ($cmd$) prevents quote-escaping hell for the internal strings
FOREACH sql_cmd IN ARRAY ARRAY [
        $cmd$ CREATE TYPE product_size AS ENUM ('S', 'M', 'L') $cmd$,
        $cmd$ CREATE TYPE bill_order_type AS ENUM ('dine_in', 'takeaway') $cmd$,
        $cmd$ CREATE TYPE bill_status AS ENUM ('pending', 'paid', 'canceled') $cmd$,
        $cmd$ CREATE TYPE user_role AS ENUM ('admin', 'cashier') $cmd$
    ] LOOP BEGIN -- Execute the raw string as a SQL command
EXECUTE sql_cmd;
EXCEPTION -- Ignore gracefully if the ENUM already exists
WHEN duplicate_object THEN null;
END;
END LOOP;
END $$;
