-- MVP catalog schema (database-first) for PostgreSQL

CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    description TEXT NULL,
    delete_flag BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS products (
    id SERIAL PRIMARY KEY,
    category_id INTEGER NOT NULL,
    name VARCHAR(200) NOT NULL,
    description TEXT NULL,
    price NUMERIC(10, 2) NOT NULL DEFAULT 0,
    image_url TEXT NULL,
    delete_flag BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT fk_products_categories
        FOREIGN KEY (category_id)
        REFERENCES categories (id)
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_products_category_id ON products (category_id);
CREATE INDEX IF NOT EXISTS ix_products_delete_flag ON products (delete_flag);
CREATE INDEX IF NOT EXISTS ix_categories_delete_flag ON categories (delete_flag);

CREATE TABLE IF NOT EXISTS orders (
    id SERIAL PRIMARY KEY,
    public_id UUID NOT NULL,
    status SMALLINT NOT NULL,
    subtotal_amount NUMERIC(12, 2) NOT NULL,
    discount_amount NUMERIC(12, 2) NOT NULL DEFAULT 0,
    tax_amount NUMERIC(12, 2) NOT NULL DEFAULT 0,
    total_amount NUMERIC(12, 2) NOT NULL,
    currency CHAR(3) NOT NULL,
    customer_email VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT uq_orders_public_id UNIQUE (public_id)
);

CREATE INDEX IF NOT EXISTS ix_orders_status ON orders (status);
CREATE INDEX IF NOT EXISTS ix_orders_created_at ON orders (created_at);

CREATE TABLE IF NOT EXISTS order_items (
    id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL,
    product_id INTEGER,
    product_name VARCHAR(200) NOT NULL,
    unit_price NUMERIC(12, 2) NOT NULL,
    quantity INTEGER NOT NULL,
    line_total NUMERIC(12, 2) NOT NULL,
    CONSTRAINT fk_order_items_orders
        FOREIGN KEY (order_id)
        REFERENCES orders (id)
        ON DELETE CASCADE,
    CONSTRAINT fk_order_items_products
        FOREIGN KEY (product_id)
        REFERENCES products (id)
        ON DELETE SET NULL,
    CONSTRAINT ck_order_items_quantity_positive
        CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_order_items_order_id ON order_items (order_id);
CREATE INDEX IF NOT EXISTS ix_order_items_product_id ON order_items (product_id);

CREATE TABLE IF NOT EXISTS processed_stripe_events (
    id SERIAL PRIMARY KEY,
    stripe_event_id TEXT NOT NULL,
    event_type TEXT NOT NULL,
    order_public_id UUID NULL,
    payment_intent_id TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_processed_stripe_events_stripe_event_id UNIQUE (stripe_event_id)
);

CREATE TABLE IF NOT EXISTS idempotency_keys (
    id SERIAL PRIMARY KEY,
    idempotency_key TEXT NOT NULL,
    scope TEXT NOT NULL,
    request_hash TEXT NULL,
    status TEXT NOT NULL
    CHECK (status IN ('processing', 'completed', 'failed')),
    response_code INTEGER NULL,
    response_body JSONB NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    completed_at TIMESTAMPTZ NULL,
    CONSTRAINT uq_idempotency_keys_key_scope UNIQUE (idempotency_key, scope)
);

CREATE INDEX IF NOT EXISTS ix_idempotency_keys_created_at ON idempotency_keys (created_at);
CREATE INDEX IF NOT EXISTS ix_idempotency_keys_status ON idempotency_keys (status);