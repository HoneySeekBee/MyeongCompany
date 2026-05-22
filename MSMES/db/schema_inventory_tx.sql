-- ============================================================
-- InventoryTransactions 테이블 생성 + StockBefore/After 컬럼 추가
-- + 샘플 데이터 15건
-- Target DB  : MSMES  (localdb)\MSSQLLocalDB
-- ============================================================

USE MSMES;
GO

-- 1. InventoryTransactions 테이블 생성 (없으면)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InventoryTransactions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.InventoryTransactions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        ItemCode        NVARCHAR(50)    NOT NULL,
        ItemName        NVARCHAR(200)   NOT NULL DEFAULT '',
        WarehouseCode   NVARCHAR(50)    NOT NULL,
        TransactionType INT             NOT NULL,  -- 0=Receipt(입고), 1=Issue(출고), 2=Adjustment(조정)
        Quantity        DECIMAL(18,2)   NOT NULL,  -- 실제 변화량 (Adjustment면 diff)
        StockBefore     DECIMAL(18,2)   NOT NULL DEFAULT 0,
        StockAfter      DECIMAL(18,2)   NOT NULL DEFAULT 0,
        ReferenceNo     NVARCHAR(100)   NULL,
        Remarks         NVARCHAR(500)   NULL,
        TransactionDate DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy       NVARCHAR(100)   NULL
    );
    PRINT N'Table InventoryTransactions created.';
END
ELSE
    PRINT N'Table InventoryTransactions already exists.';
GO

-- 2. 기존 테이블에 ItemName, StockBefore, StockAfter 컬럼 추가 (없으면)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.InventoryTransactions') AND name = N'ItemName'
)
BEGIN
    ALTER TABLE dbo.InventoryTransactions
        ADD ItemName NVARCHAR(200) NOT NULL DEFAULT '';
    PRINT N'Column ItemName added.';
END
ELSE
    PRINT N'Column ItemName already exists.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.InventoryTransactions') AND name = N'StockBefore'
)
BEGIN
    ALTER TABLE dbo.InventoryTransactions
        ADD StockBefore DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT N'Column StockBefore added.';
END
ELSE
    PRINT N'Column StockBefore already exists.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.InventoryTransactions') AND name = N'StockAfter'
)
BEGIN
    ALTER TABLE dbo.InventoryTransactions
        ADD StockAfter DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT N'Column StockAfter added.';
END
ELSE
    PRINT N'Column StockAfter already exists.';
GO

-- 3. 샘플 데이터 15건 (다양한 타입)
--    품목코드는 dbo.Inventories에 실제 존재하는 값을 참조
--    없으면 단순 샘플 코드 사용
IF NOT EXISTS (SELECT 1 FROM dbo.InventoryTransactions)
BEGIN
    DECLARE @now DATETIME2 = SYSUTCDATETIME();

    INSERT INTO dbo.InventoryTransactions
        (ItemCode, ItemName, WarehouseCode, TransactionType, Quantity, StockBefore, StockAfter, ReferenceNo, Remarks, TransactionDate, CreatedBy, CreatedAt)
    VALUES
        -- 입고 (Receipt = 0)
        (N'ITEM-001', N'볼트 M10×30', N'WH-01', 0,  500.00, 100.00,  600.00, N'PO-2024-001', N'구매입고',          DATEADD(DAY,-14, @now), N'김창고', DATEADD(DAY,-14,@now)),
        (N'ITEM-002', N'너트 M10',    N'WH-01', 0, 1000.00, 200.00, 1200.00, N'PO-2024-002', N'구매입고',          DATEADD(DAY,-13, @now), N'김창고', DATEADD(DAY,-13,@now)),
        (N'ITEM-003', N'와셔 20mm',   N'WH-02', 0,  300.00,  50.00,  350.00, N'PO-2024-003', N'긴급 발주 입고',    DATEADD(DAY,-12, @now), N'이입고', DATEADD(DAY,-12,@now)),
        (N'ITEM-001', N'볼트 M10×30', N'WH-01', 0,  200.00, 600.00,  800.00, N'PO-2024-004', N'추가 구매입고',     DATEADD(DAY,-10, @now), N'김창고', DATEADD(DAY,-10,@now)),
        (N'ITEM-004', N'스프링 소',   N'WH-02', 0,  150.00,  20.00,  170.00, N'PO-2024-005', N'정기 발주 입고',    DATEADD(DAY, -9, @now), N'이입고', DATEADD(DAY, -9,@now)),

        -- 출고 (Issue = 1)
        (N'ITEM-001', N'볼트 M10×30', N'WH-01', 1,  100.00, 800.00,  700.00, N'WO-2024-101', N'작업지시 출고',     DATEADD(DAY, -8, @now), N'박출고', DATEADD(DAY, -8,@now)),
        (N'ITEM-002', N'너트 M10',    N'WH-01', 1,  200.00,1200.00, 1000.00, N'WO-2024-102', N'라인A 투입',        DATEADD(DAY, -7, @now), N'박출고', DATEADD(DAY, -7,@now)),
        (N'ITEM-003', N'와셔 20mm',   N'WH-02', 1,   80.00, 350.00,  270.00, N'WO-2024-103', N'조립공정 출고',     DATEADD(DAY, -6, @now), N'최출고', DATEADD(DAY, -6,@now)),
        (N'ITEM-001', N'볼트 M10×30', N'WH-01', 1,  150.00, 700.00,  550.00, N'WO-2024-104', N'긴급출고',          DATEADD(DAY, -5, @now), N'박출고', DATEADD(DAY, -5,@now)),
        (N'ITEM-004', N'스프링 소',   N'WH-02', 1,   50.00, 170.00,  120.00, N'WO-2024-105', N'설비A 교체용 출고', DATEADD(DAY, -4, @now), N'최출고', DATEADD(DAY, -4,@now)),

        -- 조정 (Adjustment = 2)
        (N'ITEM-002', N'너트 M10',    N'WH-01', 2,   50.00,1000.00, 1050.00, NULL,            N'실사 후 플러스 조정',  DATEADD(DAY, -3, @now), N'재고담당', DATEADD(DAY,-3,@now)),
        (N'ITEM-003', N'와셔 20mm',   N'WH-02', 2,  -20.00, 270.00,  250.00, NULL,            N'파손 감량 조정',        DATEADD(DAY, -2, @now), N'재고담당', DATEADD(DAY,-2,@now)),
        (N'ITEM-001', N'볼트 M10×30', N'WH-01', 2,   30.00, 550.00,  580.00, NULL,            N'정기 재고실사 조정',   DATEADD(DAY, -1, @now), N'재고담당', DATEADD(DAY,-1,@now)),

        -- 오늘 데이터 (KPI 카드용)
        (N'ITEM-002', N'너트 M10',    N'WH-01', 0,  300.00,1050.00, 1350.00, N'PO-2024-010', N'오늘 입고',          @now,                   N'김창고',   @now),
        (N'ITEM-004', N'스프링 소',   N'WH-02', 1,   30.00, 120.00,   90.00, N'WO-2024-120', N'오늘 출고',          @now,                   N'박출고',   @now);

    PRINT N'Sample data (15 rows) inserted.';
END
ELSE
    PRINT N'InventoryTransactions already has data — skipping sample insert.';
GO

PRINT N'schema_inventory_tx.sql complete.';
GO
