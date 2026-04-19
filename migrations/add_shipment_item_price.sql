-- Миграция: Добавление колонки price в таблицу shipment_items
-- Эта миграция добавляет поле цены товара в момент отгрузки

BEGIN;

ALTER TABLE shipment_items
ADD COLUMN IF NOT EXISTS price NUMERIC(10, 2) NOT NULL DEFAULT 0;

-- Обновляем цены для существующих позиций (используем purchase_price товара)
UPDATE shipment_items si
SET price = COALESCE(p.purchase_price, 0)
FROM products p
WHERE si.product_id = p.id AND si.price = 0;

COMMIT;
