-- Миграция: Добавление таблицы истории поставок (опционально)
-- Эта таблица ведет учет всех поставок товаров в систему

BEGIN;

-- Таблица истории поставок (если потребуется ведение учета)
CREATE TABLE IF NOT EXISTS purchase_history (
    id          UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id  UUID            NOT NULL REFERENCES products(id),
    user_id     UUID            NOT NULL REFERENCES users(id),
    quantity    INT             NOT NULL,
    price       NUMERIC(10, 2)  NOT NULL,
    created_at  TIMESTAMP       NOT NULL DEFAULT NOW()
);

-- Индекс для быстрого поиска по товару и дате
CREATE INDEX IF NOT EXISTS idx_purchase_history_product_date 
    ON purchase_history(product_id, created_at DESC);

COMMIT;
