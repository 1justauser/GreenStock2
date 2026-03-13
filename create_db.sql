-- ============================================================
--  GreenStock — PostgreSQL setup script
--  Run this once to create tables and seed initial data
-- ============================================================

-- 1. Create tables

CREATE TABLE IF NOT EXISTS users (
    id            SERIAL PRIMARY KEY,
    login         VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role          VARCHAR(50)  NOT NULL  -- 'Admin' or 'Kladovshik'
);

CREATE TABLE IF NOT EXISTS categories (
    id   SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS products (
    id             SERIAL PRIMARY KEY,
    article        VARCHAR(50)    NOT NULL UNIQUE,
    name           VARCHAR(200)   NOT NULL,
    category_id    INT            NOT NULL REFERENCES categories(id),
    unit           VARCHAR(20)    NOT NULL,  -- шт, пак, кг, л, г
    purchase_price NUMERIC(10, 2) NOT NULL DEFAULT 0,
    stock          INT            NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS shipments (
    id         SERIAL PRIMARY KEY,
    created_by INT       NOT NULL REFERENCES users(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS shipment_items (
    id          SERIAL PRIMARY KEY,
    shipment_id INT NOT NULL REFERENCES shipments(id) ON DELETE CASCADE,
    product_id  INT NOT NULL REFERENCES products(id),
    quantity    INT NOT NULL
);

-- ============================================================
-- 2. Seed data
-- ============================================================

-- Users
-- Passwords are bcrypt hashes:
--   admin    → Admin123
--   sklad1   → Pass1234
INSERT INTO users (login, password_hash, role) VALUES
    ('admin',  '$2a$11$KzQy1KjxP9bVkL8mN3oRuOWq1tYcXvZ5eA6dF0gH2iJ4lM7nP8qRs', 'Admin'),
    ('sklad1', '$2a$11$AbCdEfGhIjKlMnOpQrStUuVwXyZ0123456789abcdefghijklmnopq', 'Kladovshik')
ON CONFLICT (login) DO NOTHING;

-- Categories
INSERT INTO categories (name) VALUES
    ('Растения садовые'),
    ('Семена овощей'),
    ('Семена цветов'),
    ('Удобрения'),
    ('Грунты и субстраты')
ON CONFLICT DO NOTHING;

-- Products (sample data)
INSERT INTO products (article, name, category_id, unit, purchase_price, stock) VALUES
    ('ROSE-001', 'Роза чайная',          1, 'шт', 150.00, 50),
    ('SEED-042', 'Томат Черри',          2, 'пак', 45.00, 120),
    ('FLOW-007', 'Петуния ампельная',    3, 'пак', 30.00, 80),
    ('FERT-003', 'Нитроаммофоска 1кг',  4, 'кг',  85.00, 30),
    ('SOIL-012', 'Грунт универсальный', 5, 'кг',  55.00, 200)
ON CONFLICT (article) DO NOTHING;

-- ============================================================
-- 3. Verify
-- ============================================================
SELECT 'users'     AS tbl, COUNT(*) FROM users
UNION ALL
SELECT 'categories',        COUNT(*) FROM categories
UNION ALL
SELECT 'products',          COUNT(*) FROM products;
