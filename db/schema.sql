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
