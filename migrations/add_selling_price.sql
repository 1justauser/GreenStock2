-- Миграция: Добавление цены продажи и таблиц для сроков годности
-- Запускать на существующей БД (если уже работала с предыдущей версией)

BEGIN;

-- 1. Добавляем цену продажи в товары
ALTER TABLE products
    ADD COLUMN IF NOT EXISTS selling_price NUMERIC(10, 2) NOT NULL DEFAULT 0;

-- По умолчанию ставим +30% к закупочной цене для существующих товаров
UPDATE products
    SET selling_price = ROUND(purchase_price * 1.3, 2)
    WHERE selling_price = 0 AND purchase_price > 0;

-- 2. Добавляем колонку price в shipment_items (если не была добавлена ранее)
ALTER TABLE shipment_items
    ADD COLUMN IF NOT EXISTS price NUMERIC(10, 2) NOT NULL DEFAULT 0;

-- Заполняем price из purchase_price товара для старых записей
UPDATE shipment_items si
    SET price = COALESCE(p.selling_price, p.purchase_price, 0)
    FROM products p
    WHERE si.product_id = p.id AND si.price = 0;

-- 3. Таблица партий товаров (для учёта сроков годности по партиям)
CREATE TABLE IF NOT EXISTS stock_batches (
    id             UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id     UUID           NOT NULL REFERENCES products(id),
    quantity       INT            NOT NULL DEFAULT 0,
    purchase_price NUMERIC(10, 2) NOT NULL DEFAULT 0,
    expiry_date    DATE           NULL,
    created_at     TIMESTAMP      NOT NULL DEFAULT NOW()
);

-- 4. Таблица списаний просроченного товара
CREATE TABLE IF NOT EXISTS write_offs (
    id         UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    batch_id   UUID         NOT NULL REFERENCES stock_batches(id),
    quantity   INT          NOT NULL,
    reason     VARCHAR(255) NOT NULL DEFAULT 'Срок годности истёк',
    created_at TIMESTAMP    NOT NULL DEFAULT NOW()
);

COMMIT;

-- Проверка
SELECT
    'products'       AS table_name, COUNT(*) FROM products
UNION ALL SELECT
    'stock_batches',                COUNT(*) FROM stock_batches
UNION ALL SELECT
    'write_offs',                   COUNT(*) FROM write_offs;
