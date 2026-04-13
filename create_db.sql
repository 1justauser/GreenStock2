-- ============================================================
--  GreenStock — PostgreSQL setup script
--  Run this once to create tables and seed initial data.
--  All primary/foreign keys use UUID instead of SERIAL.
-- ============================================================

-- 1. Enable uuid-ossp extension (needed for gen_random_uuid())
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 2. Create tables

CREATE TABLE IF NOT EXISTS users (
    id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    login         VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role          VARCHAR(50)  NOT NULL  -- 'Admin' or 'Kladovshik'
);

CREATE TABLE IF NOT EXISTS categories (
    id   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS products (
    id             UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
    article        VARCHAR(50)    NOT NULL UNIQUE,
    name           VARCHAR(200)   NOT NULL,
    category_id    UUID           NOT NULL REFERENCES categories(id),
    unit           VARCHAR(20)    NOT NULL,  -- шт, пак, кг, л, г
    purchase_price NUMERIC(10, 2) NOT NULL DEFAULT 0,
    stock          INT            NOT NULL DEFAULT 0,
    expiry_date    DATE           NULL       -- NULL = бессрочный товар
);

CREATE TABLE IF NOT EXISTS shipments (
    id         UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    created_by UUID         NOT NULL REFERENCES users(id),
    created_at TIMESTAMP    NOT NULL DEFAULT NOW(),
    recipient  VARCHAR(200) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS shipment_items (
    id          UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    shipment_id UUID NOT NULL REFERENCES shipments(id) ON DELETE CASCADE,
    product_id  UUID NOT NULL REFERENCES products(id),
    quantity    INT  NOT NULL
);

-- ============================================================
-- 3. Seed data
-- ============================================================

-- Users (bcrypt hashes, cost=11):
--   admin   → Admin123
--   sklad1  → Pass1234
INSERT INTO users (login, password_hash, role) VALUES
    ('admin',  '$2a$12$jU2OuZ9ltQMXVhotKKxhUelfjOH7fAiX9f8BwlbMKFpBl049CH/qO', 'Admin'),
    ('sklad1', '$2a$11$PW4eh3adCFzsumNXlU/.j.MoJNK5dfbbLjC0V0GQRMxt1V/3VukLG', 'Kladovshik')
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
INSERT INTO products (article, name, category_id, unit, purchase_price, stock, expiry_date)
SELECT 'ROSE-001', 'Роза чайная',         id, 'шт',  150.00, 50,  NULL        FROM categories WHERE name = 'Растения садовые'  LIMIT 1
ON CONFLICT (article) DO NOTHING;

INSERT INTO products (article, name, category_id, unit, purchase_price, stock, expiry_date)
SELECT 'SEED-042', 'Томат Черри',          id, 'пак',  45.00, 120, '2027-12-31' FROM categories WHERE name = 'Семена овощей'     LIMIT 1
ON CONFLICT (article) DO NOTHING;

INSERT INTO products (article, name, category_id, unit, purchase_price, stock, expiry_date)
SELECT 'FLOW-007', 'Петуния ампельная',    id, 'пак',  30.00, 80,  '2026-12-31' FROM categories WHERE name = 'Семена цветов'     LIMIT 1
ON CONFLICT (article) DO NOTHING;

INSERT INTO products (article, name, category_id, unit, purchase_price, stock, expiry_date)
SELECT 'FERT-003', 'Нитроаммофоска 1кг',  id, 'кг',   85.00, 30,  NULL        FROM categories WHERE name = 'Удобрения'          LIMIT 1
ON CONFLICT (article) DO NOTHING;

INSERT INTO products (article, name, category_id, unit, purchase_price, stock, expiry_date)
SELECT 'SOIL-012', 'Грунт универсальный', id, 'кг',   55.00, 200, NULL        FROM categories WHERE name = 'Грунты и субстраты' LIMIT 1
ON CONFLICT (article) DO NOTHING;

-- ============================================================
-- 4. Verify
-- ============================================================
SELECT 'users'     AS tbl, COUNT(*) FROM users
UNION ALL
SELECT 'categories',        COUNT(*) FROM categories
UNION ALL
SELECT 'products',          COUNT(*) FROM products;
