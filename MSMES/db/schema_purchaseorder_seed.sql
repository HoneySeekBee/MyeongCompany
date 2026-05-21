-- ============================================================
-- 발주(PurchaseOrders) 샘플 데이터
-- 이미 존재하는 행은 건너뜀 (MERGE / NOT EXISTS 패턴)
-- ============================================================

-- NumberSequences 초기값 보장
IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE SequenceKey = 'PO')
    INSERT INTO dbo.NumberSequences (SequenceKey, CurrentValue) VALUES ('PO', 0);

-- ── 발주 헤더 5건 ────────────────────────────────────────────
-- Draft 2건, Issued 2건, Received 1건

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE PurchaseOrderNo = 'PO2026052200001')
INSERT INTO dbo.PurchaseOrders
    (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate,
     Status, AssignedTo, Note, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200001', 'SUP-001', '(주)한국부품', '2026-05-20', '2026-06-05',
     0, '김철수', '긴급 발주 건', SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE PurchaseOrderNo = 'PO2026052200002')
INSERT INTO dbo.PurchaseOrders
    (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate,
     Status, AssignedTo, Note, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200002', 'SUP-002', '삼성부품공업', '2026-05-21', '2026-06-10',
     0, '이영희', NULL, SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE PurchaseOrderNo = 'PO2026052200003')
INSERT INTO dbo.PurchaseOrders
    (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate,
     Status, AssignedTo, Note, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200003', 'SUP-003', '대성금속(주)', '2026-05-15', '2026-05-30',
     1, '박민수', '발행 완료', SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE PurchaseOrderNo = 'PO2026052200004')
INSERT INTO dbo.PurchaseOrders
    (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate,
     Status, AssignedTo, Note, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200004', 'SUP-001', '(주)한국부품', '2026-05-10', '2026-05-25',
     1, '김철수', '납기 확인 필요', SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE PurchaseOrderNo = 'PO2026052200005')
INSERT INTO dbo.PurchaseOrders
    (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate,
     Status, AssignedTo, Note, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200005', 'SUP-004', '우진전자부품', '2026-05-01', '2026-05-15',
     3, '이영희', '입고 완료 처리됨', SYSUTCDATETIME(), 'seed');

-- ── 발주 품목 ────────────────────────────────────────────────
-- PO2026052200001: 3개 품목 (Draft)
IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200001' AND ItemNo=1)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200001', 1, 'ITEM-A001', 'A형 볼트 M6', 500, 120, 0, SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200001' AND ItemNo=2)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200001', 2, 'ITEM-A002', 'A형 너트 M6', 500, 80, 0, SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200001' AND ItemNo=3)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200001', 3, 'ITEM-B010', '스프링 와셔', 1000, 30, 0, SYSUTCDATETIME(), 'seed');

-- PO2026052200002: 2개 품목 (Draft)
IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200002' AND ItemNo=1)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200002', 1, 'ITEM-C020', 'PCB 기판 A타입', 100, 8500, 0, SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200002' AND ItemNo=2)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200002', 2, 'ITEM-C021', 'PCB 기판 B타입', 50, 12000, 0, SYSUTCDATETIME(), 'seed');

-- PO2026052200003: 2개 품목 (Issued)
IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200003' AND ItemNo=1)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200003', 1, 'ITEM-D030', '알루미늄 프레임 1m', 200, 3500, 0, SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200003' AND ItemNo=2)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200003', 2, 'ITEM-D031', '스틸 파이프 50A', 100, 5200, 0, SYSUTCDATETIME(), 'seed');

-- PO2026052200004: 3개 품목 (Issued)
IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200004' AND ItemNo=1)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200004', 1, 'ITEM-E040', '고무 패킹 Ø20', 300, 450, 0, SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200004' AND ItemNo=2)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200004', 2, 'ITEM-E041', 'O링 Ø25', 500, 200, 0, SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200004' AND ItemNo=3)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200004', 3, 'ITEM-E042', '씰링 테이프', 100, 800, 0, SYSUTCDATETIME(), 'seed');

-- PO2026052200005: 2개 품목 (Received)
IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200005' AND ItemNo=1)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200005', 1, 'ITEM-F050', '저항 10kΩ (1/4W)', 2000, 25, 2000, SYSUTCDATETIME(), 'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrderItems WHERE PurchaseOrderNo='PO2026052200005' AND ItemNo=2)
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
VALUES
    ('PO2026052200005', 2, 'ITEM-F051', '커패시터 100μF', 1000, 60, 1000, SYSUTCDATETIME(), 'seed');

-- NumberSequences 시퀀스 값 동기화 (5 이상 보장)
UPDATE dbo.NumberSequences
SET CurrentValue = CASE WHEN CurrentValue < 5 THEN 5 ELSE CurrentValue END
WHERE SequenceKey = 'PO';
