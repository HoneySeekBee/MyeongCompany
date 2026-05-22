-- ============================================================
-- 통합 샘플 데이터 (Full Sample Data)
-- Target DB  : MSMES  (localdb)\MSSQLLocalDB
-- 이미 존재하는 행은 건너뜀 (IF NOT EXISTS / NOT EXISTS 패턴)
-- ============================================================
-- 포함 항목:
--   1. dbo.Inventories   — 10개 품목 (안전재고 미달 4개 포함)
--   2. dbo.WorkOrders    — 추가 5건 (총 ~10건, 4~6월 2026 기간)
--   3. dbo.PurchaseOrders — 추가 3건 (다양한 상태)
-- ============================================================

USE MSMES;
GO

-- ============================================================
-- 1. Inventories — 10개 품목
--    컬럼: ItemCode, ItemName, WarehouseCode, CurrentStock,
--           SafetyStock, MaxStock, Unit, WarehouseLocation
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-ENG-001')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-ENG-001', N'엔진 브래킷 A형', N'WH-A', 45, 20, 100, N'EA', N'A-01', SYSUTCDATETIME(), N'seed');

-- 부족 경고: CurrentStock(8) < SafetyStock(50)
IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-BOLT-M12')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-BOLT-M12', N'볼트 M12x40', N'WH-B', 8, 50, 500, N'EA', N'B-03', SYSUTCDATETIME(), N'seed');

-- 부족 경고: CurrentStock(3) < SafetyStock(10)
IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-HSG-B')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-HSG-B', N'하우징 케이스 B형', N'WH-A', 3, 10, 50, N'EA', N'A-02', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-SHAFT-ASM')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-SHAFT-ASM', N'샤프트 조립품', N'WH-C', 22, 15, 60, N'EA', N'C-01', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-NUT-M8')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-NUT-M8', N'너트 M8 아연도금', N'WH-B', 156, 100, 500, N'EA', N'B-01', SYSUTCDATETIME(), N'seed');

-- 부족 경고: CurrentStock(12) < SafetyStock(20)
IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-PCB-001')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-PCB-001', N'PCB 기판 A형', N'WH-D', 12, 20, 100, N'EA', N'D-02', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-FRAME-01')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-FRAME-01', N'알루미늄 프레임', N'WH-A', 34, 10, 80, N'EA', N'A-03', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-CABLE-USB')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-CABLE-USB', N'USB 케이블 어셈블리', N'WH-D', 67, 30, 150, N'EA', N'D-01', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-SPRING-01')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-SPRING-01', N'압축 스프링 S5', N'WH-B', 200, 50, 400, N'EA', N'B-02', SYSUTCDATETIME(), N'seed');

-- 부족 경고: CurrentStock(5) < SafetyStock(15)
IF NOT EXISTS (SELECT 1 FROM dbo.Inventories WHERE ItemCode = N'ITEM-GEAR-M3')
    INSERT INTO dbo.Inventories
        (ItemCode, ItemName, WarehouseCode, CurrentStock, SafetyStock, MaxStock, Unit, WarehouseLocation, CreatedAt, CreatedBy)
    VALUES
        (N'ITEM-GEAR-M3', N'기어 모듈 M3', N'WH-C', 5, 15, 60, N'EA', N'C-02', SYSUTCDATETIME(), N'seed');

PRINT N'[1] Inventories 샘플 데이터 처리 완료.';
GO

-- ============================================================
-- 2. WorkOrders — 추가 5건 (WO-2026-0006 ~ WO-2026-0010)
--    기존 5건(WO-2026-0001~0005)은 schema_workorder_upgrade.sql 참조
--    WorkOrderStatus: Planned=0, Released=1, InProgress=2,
--                     Completed=3, Closed=4, Cancelled=5
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0006')
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode,
         PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate,
         Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0006', N'SO-2026-0001', N'ITEM-PCB-001', N'PCB 기판 A형', N'PROC-SMT',
         400, 0,
         '2026-04-01', '2026-04-15', NULL,
         0 /*Planned*/, N'SMT 공정 — 4월 초도 계획', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0007')
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode,
         PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate,
         Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0007', N'SO-2026-0002', N'ITEM-FRAME-01', N'알루미늄 프레임', N'PROC-MILL',
         600, 320,
         '2026-04-10', '2026-04-30', NULL,
         2 /*InProgress*/, N'밀링 가공 진행 중 — 53% 완료', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0008')
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode,
         PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate,
         Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0008', N'SO-2026-0003', N'ITEM-GEAR-M3', N'기어 모듈 M3', N'PROC-GRIND',
         250, 250,
         '2026-04-20', '2026-05-05', '2026-05-04',
         3 /*Completed*/, N'연삭 공정 정상 완료 (1일 조기 완료)', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0009')
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode,
         PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate,
         Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0009', N'SO-2026-0004', N'ITEM-CABLE-USB', N'USB 케이블 어셈블리', N'PROC-ASM',
         1200, 1200,
         '2026-05-01', '2026-05-18', '2026-05-17',
         4 /*Closed*/, N'조립 및 테스트 완료, 마감 처리', SYSUTCDATETIME(), N'seed');

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0010')
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode,
         PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate,
         Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0010', N'SO-2026-0005', N'ITEM-SPRING-01', N'압축 스프링 S5', N'PROC-COIL',
         800, 0,
         '2026-06-01', '2026-06-20', NULL,
         5 /*Cancelled*/, N'원자재 수급 문제로 취소 — 재일정 협의 중', SYSUTCDATETIME(), N'seed');

-- NumberSequences 동기화 (WO 시퀀스 10 이상 보장)
IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE SequenceKey = N'WO')
    INSERT INTO dbo.NumberSequences (SequenceKey, CurrentValue) VALUES (N'WO', 10);
ELSE
    UPDATE dbo.NumberSequences
    SET CurrentValue = CASE WHEN CurrentValue < 10 THEN 10 ELSE CurrentValue END
    WHERE SequenceKey = N'WO';

PRINT N'[2] WorkOrders 추가 샘플 5건 처리 완료.';
GO

-- ============================================================
-- 3. PurchaseOrders — 추가 3건 (다양한 상태)
--    PurchaseOrderStatus: Draft=0, Issued=1, PartialReceived=2, Received=3, Cancelled=4
--    (schema_purchaseorder_seed.sql 기존 5건 PO2026052200001~5 존재)
-- ============================================================

-- PO-2026-0001: 볼트 M12x40 발주 — 입고 완료 (Received = 3)
IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE PurchaseOrderNo = N'PO-2026-0001')
BEGIN
    INSERT INTO dbo.PurchaseOrders
        (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate,
         Status, AssignedTo, Note, CreatedAt, CreatedBy)
    VALUES
        (N'PO-2026-0001', N'S-002', N'대성산업', '2026-04-10', '2026-04-25',
         3 /*Received*/, N'이영희', N'볼트 M12x40 정기 발주 — 입고 완료', SYSUTCDATETIME(), N'seed');

    INSERT INTO dbo.PurchaseOrderItems
        (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
    VALUES
        (N'PO-2026-0001', 1, N'ITEM-BOLT-M12', N'볼트 M12x40', 500, 85, 500, SYSUTCDATETIME(), N'seed');
END

-- PO-2026-0002: PCB 기판 발주 — 발주 완료/배송 중 (Issued = 1)
IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE PurchaseOrderNo = N'PO-2026-0002')
BEGIN
    INSERT INTO dbo.PurchaseOrders
        (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate,
         Status, AssignedTo, Note, CreatedAt, CreatedBy)
    VALUES
        (N'PO-2026-0002', N'S-003', N'신성전자', '2026-05-10', '2026-05-28',
         1 /*Issued*/, N'박민수', N'PCB 기판 A형 긴급 발주 — 납기 준수 필수', SYSUTCDATETIME(), N'seed');

    INSERT INTO dbo.PurchaseOrderItems
        (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
    VALUES
        (N'PO-2026-0002', 1, N'ITEM-PCB-001', N'PCB 기판 A형', 100, 8500, 0, SYSUTCDATETIME(), N'seed');
END

-- PO-2026-0003: 기어 모듈 발주 — 초안 (Draft = 0)
IF NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders WHERE PurchaseOrderNo = N'PO-2026-0003')
BEGIN
    INSERT INTO dbo.PurchaseOrders
        (PurchaseOrderNo, SupplierCode, SupplierName, OrderDate, DueDate,
         Status, AssignedTo, Note, CreatedAt, CreatedBy)
    VALUES
        (N'PO-2026-0003', N'S-001', N'한국부품(주)', '2026-05-20', '2026-06-10',
         0 /*Draft*/, N'김철수', N'기어 모듈 M3 견적 검토 후 발행 예정', SYSUTCDATETIME(), N'seed');

    INSERT INTO dbo.PurchaseOrderItems
        (PurchaseOrderNo, ItemNo, ItemCode, ItemName, OrderQuantity, UnitPrice, ReceivedQuantity, CreatedAt, CreatedBy)
    VALUES
        (N'PO-2026-0003', 1, N'ITEM-GEAR-M3', N'기어 모듈 M3', 60, 45000, 0, SYSUTCDATETIME(), N'seed');
END

-- NumberSequences 동기화 (PO 시퀀스 충분히 보장 — 기존 seed 포함 8건)
UPDATE dbo.NumberSequences
SET CurrentValue = CASE WHEN CurrentValue < 8 THEN 8 ELSE CurrentValue END
WHERE SequenceKey = N'PO';

PRINT N'[3] PurchaseOrders 추가 샘플 3건 처리 완료.';
GO

PRINT N'====================================================';
PRINT N'schema_sample_data_full.sql 실행 완료.';
PRINT N'====================================================';
GO
