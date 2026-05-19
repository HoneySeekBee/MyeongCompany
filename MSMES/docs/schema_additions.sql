-- =====================================================================
-- MSMES Schema Additions: Inventory / Quality / Equipment / Process
-- Target: SQL Server 2019+
-- Depends on: docs/schema.sql (must be executed first)
-- =====================================================================
USE MSMES;
GO

-- ---------------------------------------------------------------------
-- Inventory: 재고관리
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.Inventories','U') IS NULL
CREATE TABLE dbo.Inventories (
    ItemCode        NVARCHAR(50)    NOT NULL,
    WarehouseCode   NVARCHAR(50)    NOT NULL,
    ItemName        NVARCHAR(200)   NOT NULL,
    CurrentStock    DECIMAL(18,4)   NOT NULL DEFAULT 0,
    SafetyStock     DECIMAL(18,4)   NOT NULL DEFAULT 0,
    Unit            NVARCHAR(20)    NOT NULL DEFAULT 'EA',
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT PK_Inventories PRIMARY KEY (ItemCode, WarehouseCode)
);
GO

IF OBJECT_ID('dbo.InventoryTransactions','U') IS NULL
CREATE TABLE dbo.InventoryTransactions (
    TransactionId   BIGINT          IDENTITY(1,1) PRIMARY KEY,
    ItemCode        NVARCHAR(50)    NOT NULL,
    WarehouseCode   NVARCHAR(50)    NOT NULL,
    TransactionType INT             NOT NULL, -- 0=Receipt, 1=Issue, 2=Adjustment
    Quantity        DECIMAL(18,4)   NOT NULL,
    ReferenceNo     NVARCHAR(50)    NULL,
    TransactionDate DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    Remarks         NVARCHAR(500)   NULL,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL
);
CREATE INDEX IX_InvTx_Item ON dbo.InventoryTransactions(ItemCode, TransactionDate DESC);
GO

-- ---------------------------------------------------------------------
-- Quality: 품질관리
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.DefectTypes','U') IS NULL
CREATE TABLE dbo.DefectTypes (
    DefectTypeCode  NVARCHAR(50)    NOT NULL PRIMARY KEY,
    DefectTypeName  NVARCHAR(200)   NOT NULL,
    DefectCause     NVARCHAR(500)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL
);
GO

IF OBJECT_ID('dbo.QualityInspections','U') IS NULL
CREATE TABLE dbo.QualityInspections (
    InspectionNo        NVARCHAR(50)    NOT NULL PRIMARY KEY,
    LotNo               NVARCHAR(50)    NOT NULL,
    InspectionItem      NVARCHAR(200)   NOT NULL,
    Result              INT             NOT NULL, -- 0=Pending, 1=Passed, 2=Failed, 3=ConditionalPass
    InspectedQuantity   DECIMAL(18,4)   NOT NULL DEFAULT 0,
    DefectQuantity      DECIMAL(18,4)   NOT NULL DEFAULT 0,
    DefectTypeCode      NVARCHAR(50)    NULL,
    InspectionDate      DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    Inspector           NVARCHAR(100)   NOT NULL,
    Remarks             NVARCHAR(500)   NULL,
    CreatedAt           DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2(3)    NULL,
    CreatedBy           NVARCHAR(50)    NULL
);
CREATE INDEX IX_QI_Lot ON dbo.QualityInspections(LotNo);
CREATE INDEX IX_QI_Date ON dbo.QualityInspections(InspectionDate DESC);
GO

-- ---------------------------------------------------------------------
-- Equipment: 설비관리
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.Equipments','U') IS NULL
CREATE TABLE dbo.Equipments (
    EquipmentCode       NVARCHAR(50)    NOT NULL PRIMARY KEY,
    EquipmentName       NVARCHAR(200)   NOT NULL,
    EquipmentType       NVARCHAR(100)   NOT NULL,
    Location            NVARCHAR(200)   NULL,
    Status              INT             NOT NULL DEFAULT 1, -- 0=Running,1=Stopped,2=Maintenance,3=Breakdown
    LastInspectionDate  DATETIME2(3)    NULL,
    NextInspectionDate  DATETIME2(3)    NULL,
    CreatedAt           DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2(3)    NULL,
    CreatedBy           NVARCHAR(50)    NULL
);
GO

IF OBJECT_ID('dbo.EquipmentMaintenances','U') IS NULL
CREATE TABLE dbo.EquipmentMaintenances (
    MaintenanceNo   NVARCHAR(50)    NOT NULL PRIMARY KEY,
    EquipmentCode   NVARCHAR(50)    NOT NULL,
    MaintenanceType NVARCHAR(50)    NOT NULL,
    Description     NVARCHAR(1000)  NULL,
    MaintenanceDate DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    Operator        NVARCHAR(100)   NOT NULL,
    NextDueDate     DATETIME2(3)    NULL,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL
);
CREATE INDEX IX_EM_Equipment ON dbo.EquipmentMaintenances(EquipmentCode, MaintenanceDate DESC);
GO

-- ---------------------------------------------------------------------
-- Process: 공정관리
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.ProcessDefinitions','U') IS NULL
CREATE TABLE dbo.ProcessDefinitions (
    ProcessCode         NVARCHAR(50)    NOT NULL PRIMARY KEY,
    ProcessName         NVARCHAR(200)   NOT NULL,
    ProcessOrder        INT             NOT NULL DEFAULT 0,
    StandardTimeMinutes DECIMAL(18,4)   NOT NULL DEFAULT 0,
    EquipmentType       NVARCHAR(100)   NULL,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2(3)    NULL,
    CreatedBy           NVARCHAR(50)    NULL
);
GO

IF OBJECT_ID('dbo.ProductionResults','U') IS NULL
CREATE TABLE dbo.ProductionResults (
    ResultNo        NVARCHAR(50)    NOT NULL PRIMARY KEY,
    WorkOrderNo     NVARCHAR(50)    NOT NULL,
    ProcessCode     NVARCHAR(50)    NOT NULL,
    Operator        NVARCHAR(100)   NOT NULL,
    ProducedQuantity DECIMAL(18,4)  NOT NULL DEFAULT 0,
    DefectQuantity  DECIMAL(18,4)   NOT NULL DEFAULT 0,
    StartTime       DATETIME2(3)    NOT NULL,
    EndTime         DATETIME2(3)    NULL,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL
);
CREATE INDEX IX_PR_WorkOrder ON dbo.ProductionResults(WorkOrderNo);
CREATE INDEX IX_PR_StartTime ON dbo.ProductionResults(StartTime DESC);
GO

-- =====================================================================
-- Sample Data
-- =====================================================================

-- Inventories
MERGE dbo.Inventories AS t
USING (VALUES
    ('ITM-001','WH-A','강판 SS400 2.0T', 1200, 300, 'EA'),
    ('ITM-002','WH-A','볼트 M8 x 20',    50,   200, 'EA'),
    ('ITM-003','WH-A','베어링 6204Z',     0,    100, 'EA'),
    ('ITM-004','WH-B','윤활유 5L',        80,   50,  'CAN'),
    ('ITM-005','WH-B','포장상자 大',      450,  100, 'EA')
) AS s(ItemCode,WarehouseCode,ItemName,CurrentStock,SafetyStock,Unit)
ON t.ItemCode = s.ItemCode AND t.WarehouseCode = s.WarehouseCode
WHEN NOT MATCHED THEN INSERT (ItemCode,WarehouseCode,ItemName,CurrentStock,SafetyStock,Unit)
    VALUES (s.ItemCode,s.WarehouseCode,s.ItemName,s.CurrentStock,s.SafetyStock,s.Unit);
GO

-- DefectTypes
MERGE dbo.DefectTypes AS t
USING (VALUES
    ('D-SCR','스크래치','표면 긁힘'),
    ('D-DIM','치수불량','가공 치수 오차'),
    ('D-CRK','크랙','용접부 균열'),
    ('D-COL','변색','열처리 변색'),
    ('D-CON','오염','이물질 부착')
) AS s(DefectTypeCode,DefectTypeName,DefectCause)
ON t.DefectTypeCode = s.DefectTypeCode
WHEN NOT MATCHED THEN INSERT (DefectTypeCode,DefectTypeName,DefectCause)
    VALUES (s.DefectTypeCode,s.DefectTypeName,s.DefectCause);
GO

-- QualityInspections
MERGE dbo.QualityInspections AS t
USING (VALUES
    ('QI-20260501-001','LOT-20260501-001','외관검사',1,100,0,   NULL,    '2026-05-01','홍길동'),
    ('QI-20260502-001','LOT-20260502-001','치수검사',2,100,3,   'D-DIM', '2026-05-02','김검사'),
    ('QI-20260510-001','LOT-20260510-001','외관검사',3,200,5,   'D-SCR', '2026-05-10','홍길동'),
    ('QI-20260515-001','LOT-20260515-001','용접검사',1,150,0,   NULL,    '2026-05-15','이품질')
) AS s(InspectionNo,LotNo,InspectionItem,Result,InspectedQuantity,DefectQuantity,DefectTypeCode,InspectionDate,Inspector)
ON t.InspectionNo = s.InspectionNo
WHEN NOT MATCHED THEN INSERT (InspectionNo,LotNo,InspectionItem,Result,InspectedQuantity,DefectQuantity,DefectTypeCode,InspectionDate,Inspector)
    VALUES (s.InspectionNo,s.LotNo,s.InspectionItem,s.Result,s.InspectedQuantity,s.DefectQuantity,s.DefectTypeCode,s.InspectionDate,s.Inspector);
GO

-- Equipments
MERGE dbo.Equipments AS t
USING (VALUES
    ('EQ-CNC-001','CNC 머시닝센터 1호','CNC','1공장 A구역',0,'2026-04-01','2026-07-01'),
    ('EQ-CNC-002','CNC 머시닝센터 2호','CNC','1공장 A구역',1,'2026-03-15','2026-06-15'),
    ('EQ-PRS-001','프레스 200T 1호',   'Press','1공장 B구역',0,'2026-04-20','2026-07-20'),
    ('EQ-WLD-001','자동용접기 1호',     'Welding','2공장 A구역',2,'2026-05-01','2026-08-01'),
    ('EQ-INS-001','3차원측정기',       'Inspection','품질동',3,'2026-02-10','2026-05-10')
) AS s(EquipmentCode,EquipmentName,EquipmentType,Location,Status,LastInspectionDate,NextInspectionDate)
ON t.EquipmentCode = s.EquipmentCode
WHEN NOT MATCHED THEN INSERT (EquipmentCode,EquipmentName,EquipmentType,Location,Status,LastInspectionDate,NextInspectionDate)
    VALUES (s.EquipmentCode,s.EquipmentName,s.EquipmentType,s.Location,s.Status,s.LastInspectionDate,s.NextInspectionDate);
GO

-- EquipmentMaintenances
MERGE dbo.EquipmentMaintenances AS t
USING (VALUES
    ('MNT-20260401-001','EQ-CNC-001','정기','월간 정기 점검 - 윤활/필터 교체','2026-04-01','박정비','2026-07-01'),
    ('MNT-20260420-001','EQ-PRS-001','정기','프레스 유압 점검','2026-04-20','박정비','2026-07-20'),
    ('MNT-20260501-001','EQ-WLD-001','예방','전극 교체 및 토치 청소','2026-05-01','최정비','2026-08-01')
) AS s(MaintenanceNo,EquipmentCode,MaintenanceType,Description,MaintenanceDate,Operator,NextDueDate)
ON t.MaintenanceNo = s.MaintenanceNo
WHEN NOT MATCHED THEN INSERT (MaintenanceNo,EquipmentCode,MaintenanceType,Description,MaintenanceDate,Operator,NextDueDate)
    VALUES (s.MaintenanceNo,s.EquipmentCode,s.MaintenanceType,s.Description,s.MaintenanceDate,s.Operator,s.NextDueDate);
GO

-- ProcessDefinitions
MERGE dbo.ProcessDefinitions AS t
USING (VALUES
    ('P-010','자재준비',     10, 15, 'Manual'),
    ('P-020','절단',         20, 30, 'CNC'),
    ('P-030','가공',         30, 60, 'CNC'),
    ('P-040','용접',         40, 45, 'Welding'),
    ('P-050','검사/포장',    50, 20, 'Inspection')
) AS s(ProcessCode,ProcessName,ProcessOrder,StandardTimeMinutes,EquipmentType)
ON t.ProcessCode = s.ProcessCode
WHEN NOT MATCHED THEN INSERT (ProcessCode,ProcessName,ProcessOrder,StandardTimeMinutes,EquipmentType)
    VALUES (s.ProcessCode,s.ProcessName,s.ProcessOrder,s.StandardTimeMinutes,s.EquipmentType);
GO

-- ProductionResults
MERGE dbo.ProductionResults AS t
USING (VALUES
    ('PR-20260518-001','WO-20260518-001','P-020','김작업',300,2,'2026-05-18 08:00','2026-05-18 12:00'),
    ('PR-20260518-002','WO-20260518-001','P-030','김작업',298,1,'2026-05-18 13:00','2026-05-18 17:30'),
    ('PR-20260519-001','WO-20260519-001','P-020','이작업',500,5,'2026-05-19 08:00','2026-05-19 14:00'),
    ('PR-20260520-001','WO-20260520-001','P-040','박작업',250,3,'2026-05-20 09:00',NULL)
) AS s(ResultNo,WorkOrderNo,ProcessCode,Operator,ProducedQuantity,DefectQuantity,StartTime,EndTime)
ON t.ResultNo = s.ResultNo
WHEN NOT MATCHED THEN INSERT (ResultNo,WorkOrderNo,ProcessCode,Operator,ProducedQuantity,DefectQuantity,StartTime,EndTime)
    VALUES (s.ResultNo,s.WorkOrderNo,s.ProcessCode,s.Operator,s.ProducedQuantity,s.DefectQuantity,s.StartTime,s.EndTime);
GO

-- NumberSequence 추가 (MNT: 점검이력, PR: 생산실적)
INSERT INTO dbo.NumberSequences (SequenceKey, CurrentValue)
SELECT v.k, 0 FROM (VALUES ('MNT'),('PR'),('EQ'),('PROC')) v(k)
WHERE NOT EXISTS (SELECT 1 FROM dbo.NumberSequences ns WHERE ns.SequenceKey = v.k);
GO

-- InventoryTransactions (sample)
INSERT INTO dbo.InventoryTransactions (ItemCode,WarehouseCode,TransactionType,Quantity,ReferenceNo,Remarks)
SELECT * FROM (VALUES
    ('ITM-001','WH-A',0,500,'PO-20260501-001','초기 입고'),
    ('ITM-002','WH-A',1,150,'WO-20260518-001','작업지시 출고'),
    ('ITM-004','WH-B',0,100,'PO-20260510-001','월간 입고')
) AS v(a,b,c,d,e,f)
WHERE NOT EXISTS (SELECT 1 FROM dbo.InventoryTransactions);
GO
