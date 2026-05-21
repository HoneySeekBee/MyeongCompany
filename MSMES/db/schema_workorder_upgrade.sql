-- ============================================================
-- WorkOrder upgrade: ItemName, ProcessCode, Note columns
-- Target DB  : MSMES  (localdb)\MSSQLLocalDB
-- Table      : dbo.WorkOrders
-- ============================================================

USE MSMES;
GO

-- 1. ItemName -----------------------------------------------
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.WorkOrders') AND name = N'ItemName'
)
BEGIN
    ALTER TABLE dbo.WorkOrders
        ADD ItemName NVARCHAR(200) NOT NULL CONSTRAINT DF_WorkOrders_ItemName DEFAULT N'';
    PRINT N'Column ItemName added.';
END
ELSE
    PRINT N'Column ItemName already exists -- skipped.';
GO

-- 2. ProcessCode --------------------------------------------
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.WorkOrders') AND name = N'ProcessCode'
)
BEGIN
    ALTER TABLE dbo.WorkOrders
        ADD ProcessCode NVARCHAR(50) NULL;
    PRINT N'Column ProcessCode added.';
END
ELSE
    PRINT N'Column ProcessCode already exists -- skipped.';
GO

-- 3. Note ---------------------------------------------------
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.WorkOrders') AND name = N'Note'
)
BEGIN
    ALTER TABLE dbo.WorkOrders
        ADD Note NVARCHAR(1000) NULL;
    PRINT N'Column Note added.';
END
ELSE
    PRINT N'Column Note already exists -- skipped.';
GO

-- 4. Cancelled enum value guard (Status=5)
-- WorkOrderStatus enum: Planned=0, Released=1, InProgress=2, Completed=3, Closed=4, Cancelled=5
-- No schema change needed; NVARCHAR/INT column already supports new value.

-- 5. Sample data (5 records) --------------------------------
-- Skip insert if WorkOrderNo already exists
IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0001')
BEGIN
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode, PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0001', N'SO-2026-0001', N'ITEM-ENG-001', N'엔진 브래킷 A형',  N'PROC-PRESS',  500,   0,
         '2026-05-01', '2026-05-10', NULL, 0 /*Planned*/,   N'1차 프레스 가공 공정', SYSUTCDATETIME(), N'system');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0002')
BEGIN
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode, PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0002', N'SO-2026-0002', N'ITEM-BOLT-M12', N'볼트 M12 x 40',   N'PROC-TURN',  2000, 0,
         '2026-05-05', '2026-05-15', NULL, 1 /*Released*/,  N'CNC 선반 가공', SYSUTCDATETIME(), N'system');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0003')
BEGIN
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode, PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0003', N'SO-2026-0003', N'ITEM-HSG-B',   N'하우징 케이스 B형', N'PROC-WELD',   300,  186,
         '2026-05-10', '2026-05-20', NULL, 2 /*InProgress*/, N'용접 후 도장 공정 포함', SYSUTCDATETIME(), N'system');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0004')
BEGIN
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode, PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0004', N'SO-2026-0004', N'ITEM-SHAFT-ASM', N'샤프트 조립품',   N'PROC-ASM',   150,  150,
         '2026-05-08', '2026-05-16', '2026-05-16', 3 /*Completed*/, N'정상 완료', SYSUTCDATETIME(), N'system');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.WorkOrders WHERE WorkOrderNo = N'WO-2026-0005')
BEGIN
    INSERT INTO dbo.WorkOrders
        (WorkOrderNo, SalesOrderNo, ItemCode, ItemName, ProcessCode, PlannedQuantity, ProducedQuantity,
         StartDate, PlannedEndDate, ActualEndDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'WO-2026-0005', NULL, N'ITEM-NUT-M8', N'너트 M8 (아연도금)', N'PROC-PLATE', 5000, 5000,
         '2026-05-03', '2026-05-12', '2026-05-12', 4 /*Closed*/, N'도금 검사 완료 후 마감', SYSUTCDATETIME(), N'system');
END
GO

PRINT N'WorkOrder upgrade complete.';
GO
