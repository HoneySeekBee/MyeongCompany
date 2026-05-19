-- =====================================================================
-- MSMES (MES) MSSQL Schema
-- Target: SQL Server 2019+
-- =====================================================================

IF DB_ID('MSMES') IS NULL CREATE DATABASE MSMES;
GO
USE MSMES;
GO

-- ---------------------------------------------------------------------
-- Common: Users
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.Users','U') IS NULL
CREATE TABLE dbo.Users (
    UserId          NVARCHAR(50)    NOT NULL PRIMARY KEY,
    Name            NVARCHAR(100)   NOT NULL,
    Email           NVARCHAR(200)   NOT NULL,
    Role            NVARCHAR(50)    NOT NULL DEFAULT 'User',
    PasswordHash    NVARCHAR(500)   NOT NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL
);
CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users(Email);
GO

-- ---------------------------------------------------------------------
-- Common: Common Codes
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.CommonCodes','U') IS NULL
CREATE TABLE dbo.CommonCodes (
    CodeGroup       NVARCHAR(50)    NOT NULL,
    Code            NVARCHAR(50)    NOT NULL,
    CodeName        NVARCHAR(200)   NOT NULL,
    SortOrder       INT             NOT NULL DEFAULT 0,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT PK_CommonCodes PRIMARY KEY (CodeGroup, Code)
);
GO

-- ---------------------------------------------------------------------
-- Sales Orders
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.SalesOrders','U') IS NULL
CREATE TABLE dbo.SalesOrders (
    SalesOrderNo    NVARCHAR(20)    NOT NULL PRIMARY KEY,
    CustomerCode    NVARCHAR(30)    NOT NULL,
    CustomerName    NVARCHAR(200)   NOT NULL,
    OrderDate       DATE            NOT NULL,
    DueDate         DATE            NOT NULL,
    Status          INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL
);
CREATE INDEX IX_SalesOrders_Customer ON dbo.SalesOrders(CustomerCode);
CREATE INDEX IX_SalesOrders_OrderDate ON dbo.SalesOrders(OrderDate);
GO

IF OBJECT_ID('dbo.SalesOrderItems','U') IS NULL
CREATE TABLE dbo.SalesOrderItems (
    SalesOrderNo    NVARCHAR(20)    NOT NULL,
    ItemNo          INT             NOT NULL,
    ItemCode        NVARCHAR(50)    NOT NULL,
    ItemName        NVARCHAR(200)   NOT NULL,
    Quantity        DECIMAL(18,4)   NOT NULL,
    UnitPrice       DECIMAL(18,4)   NOT NULL,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT PK_SalesOrderItems PRIMARY KEY (SalesOrderNo, ItemNo),
    CONSTRAINT FK_SOI_SO FOREIGN KEY (SalesOrderNo) REFERENCES dbo.SalesOrders(SalesOrderNo) ON DELETE CASCADE
);
GO

-- ---------------------------------------------------------------------
-- Purchase Orders
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.PurchaseOrders','U') IS NULL
CREATE TABLE dbo.PurchaseOrders (
    PurchaseOrderNo NVARCHAR(20)    NOT NULL PRIMARY KEY,
    SupplierCode    NVARCHAR(30)    NOT NULL,
    SupplierName    NVARCHAR(200)   NOT NULL,
    OrderDate       DATE            NOT NULL,
    DueDate         DATE            NOT NULL,
    Status          INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL
);
CREATE INDEX IX_PurchaseOrders_Supplier ON dbo.PurchaseOrders(SupplierCode);
GO

IF OBJECT_ID('dbo.PurchaseOrderItems','U') IS NULL
CREATE TABLE dbo.PurchaseOrderItems (
    PurchaseOrderNo NVARCHAR(20)    NOT NULL,
    ItemNo          INT             NOT NULL,
    ItemCode        NVARCHAR(50)    NOT NULL,
    ItemName        NVARCHAR(200)   NOT NULL,
    OrderQuantity   DECIMAL(18,4)   NOT NULL,
    UnitPrice       DECIMAL(18,4)   NOT NULL,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT PK_PurchaseOrderItems PRIMARY KEY (PurchaseOrderNo, ItemNo),
    CONSTRAINT FK_POI_PO FOREIGN KEY (PurchaseOrderNo) REFERENCES dbo.PurchaseOrders(PurchaseOrderNo) ON DELETE CASCADE
);
GO

-- ---------------------------------------------------------------------
-- Work Orders
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.WorkOrders','U') IS NULL
CREATE TABLE dbo.WorkOrders (
    WorkOrderNo     NVARCHAR(20)    NOT NULL PRIMARY KEY,
    SalesOrderNo    NVARCHAR(20)    NULL,
    ItemCode        NVARCHAR(50)    NOT NULL,
    PlannedQuantity DECIMAL(18,4)   NOT NULL,
    ProducedQuantity DECIMAL(18,4)  NOT NULL DEFAULT 0,
    StartDate       DATE            NOT NULL,
    PlannedEndDate  DATE            NOT NULL,
    ActualEndDate   DATETIME2(3)    NULL,
    Status          INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT FK_WO_SO FOREIGN KEY (SalesOrderNo) REFERENCES dbo.SalesOrders(SalesOrderNo)
);
CREATE INDEX IX_WorkOrders_Status ON dbo.WorkOrders(Status);
CREATE INDEX IX_WorkOrders_SO ON dbo.WorkOrders(SalesOrderNo);
GO

-- ---------------------------------------------------------------------
-- LOT
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.Lots','U') IS NULL
CREATE TABLE dbo.Lots (
    LotNo           NVARCHAR(30)    NOT NULL PRIMARY KEY,
    ItemCode        NVARCHAR(50)    NOT NULL,
    WorkOrderNo     NVARCHAR(20)    NOT NULL,
    ProducedQuantity DECIMAL(18,4)  NOT NULL,
    ProductionDate  DATE            NOT NULL,
    Status          INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT FK_Lot_WO FOREIGN KEY (WorkOrderNo) REFERENCES dbo.WorkOrders(WorkOrderNo)
);
CREATE INDEX IX_Lots_WorkOrder ON dbo.Lots(WorkOrderNo);
CREATE INDEX IX_Lots_Item ON dbo.Lots(ItemCode);
GO

IF OBJECT_ID('dbo.LotHistories','U') IS NULL
CREATE TABLE dbo.LotHistories (
    HistoryId       BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LotNo           NVARCHAR(30)    NOT NULL,
    Operation       NVARCHAR(100)   NOT NULL,
    OperatedAt      DATETIME2(3)    NOT NULL,
    Operator        NVARCHAR(50)    NOT NULL,
    Remarks         NVARCHAR(500)   NULL,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT FK_LH_Lot FOREIGN KEY (LotNo) REFERENCES dbo.Lots(LotNo) ON DELETE CASCADE
);
CREATE INDEX IX_LotHistories_Lot ON dbo.LotHistories(LotNo);
GO

-- ---------------------------------------------------------------------
-- Shipment
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.Shipments','U') IS NULL
CREATE TABLE dbo.Shipments (
    ShipmentNo      NVARCHAR(20)    NOT NULL PRIMARY KEY,
    SalesOrderNo    NVARCHAR(20)    NOT NULL,
    ShipmentDate    DATETIME2(3)    NOT NULL,
    DeliveryAddress NVARCHAR(500)   NOT NULL,
    Status          INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT FK_Ship_SO FOREIGN KEY (SalesOrderNo) REFERENCES dbo.SalesOrders(SalesOrderNo)
);
CREATE INDEX IX_Shipments_SO ON dbo.Shipments(SalesOrderNo);
GO

IF OBJECT_ID('dbo.ShipmentItems','U') IS NULL
CREATE TABLE dbo.ShipmentItems (
    ShipmentNo      NVARCHAR(20)    NOT NULL,
    ItemNo          INT             NOT NULL,
    LotNo           NVARCHAR(30)    NOT NULL,
    ItemCode        NVARCHAR(50)    NOT NULL,
    ShipQuantity    DECIMAL(18,4)   NOT NULL,
    CreatedAt       DATETIME2(3)    NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NULL,
    CreatedBy       NVARCHAR(50)    NULL,
    CONSTRAINT PK_ShipmentItems PRIMARY KEY (ShipmentNo, ItemNo),
    CONSTRAINT FK_SI_Ship FOREIGN KEY (ShipmentNo) REFERENCES dbo.Shipments(ShipmentNo) ON DELETE CASCADE,
    CONSTRAINT FK_SI_Lot FOREIGN KEY (LotNo) REFERENCES dbo.Lots(LotNo)
);
GO

-- ---------------------------------------------------------------------
-- Number Sequences (simple table-based sequence)
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.NumberSequences','U') IS NULL
CREATE TABLE dbo.NumberSequences (
    SequenceKey     NVARCHAR(50)    NOT NULL PRIMARY KEY,
    CurrentValue    BIGINT          NOT NULL DEFAULT 0
);
GO

INSERT INTO dbo.NumberSequences (SequenceKey, CurrentValue)
SELECT v.k, 0 FROM (VALUES ('SO'),('PO'),('WO'),('LOT'),('SHP')) v(k)
WHERE NOT EXISTS (SELECT 1 FROM dbo.NumberSequences ns WHERE ns.SequenceKey = v.k);
GO

-- ---------------------------------------------------------------------
-- Initial Data: Admin User
-- Password: Admin1234! (BCrypt hash, cost factor 11)
-- 운영 환경에서는 반드시 비밀번호를 변경하세요.
-- ---------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = 'admin')
INSERT INTO dbo.Users (UserId, Name, Email, Role, PasswordHash, IsActive, CreatedAt, CreatedBy)
VALUES (
    'admin',
    '시스템 관리자',
    'admin@msmes.local',
    'Admin',
    '$2a$11$9KDctBvz.i0D3cpDFGVgR.eY2Zl9QfM/iTK5W4bE8RWIrOonb7RpG',  -- Admin1234!
    1,
    SYSUTCDATETIME(),
    'system'
);
GO

-- ---------------------------------------------------------------------
-- Initial Data: Common Codes
-- ---------------------------------------------------------------------

-- 사용자 역할(Role)
INSERT INTO dbo.CommonCodes (CodeGroup, Code, CodeName, SortOrder, IsActive, CreatedAt, CreatedBy)
SELECT v.cg, v.c, v.cn, v.so, 1, SYSUTCDATETIME(), 'system'
FROM (VALUES
    ('ROLE', 'Admin',    '관리자',     1),
    ('ROLE', 'Manager',  '매니저',     2),
    ('ROLE', 'Operator', '작업자',     3),
    ('ROLE', 'User',     '일반 사용자', 4)
) v(cg, c, cn, so)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CommonCodes cc WHERE cc.CodeGroup = v.cg AND cc.Code = v.c
);
GO

-- 수주(SalesOrder) 상태
INSERT INTO dbo.CommonCodes (CodeGroup, Code, CodeName, SortOrder, IsActive, CreatedAt, CreatedBy)
SELECT v.cg, v.c, v.cn, v.so, 1, SYSUTCDATETIME(), 'system'
FROM (VALUES
    ('SO_STATUS', '0', '초안(Draft)',      1),
    ('SO_STATUS', '1', '확정(Confirmed)',  2),
    ('SO_STATUS', '2', '생산중(In Production)', 3),
    ('SO_STATUS', '3', '출하완료(Shipped)', 4),
    ('SO_STATUS', '4', '마감(Closed)',     5)
) v(cg, c, cn, so)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CommonCodes cc WHERE cc.CodeGroup = v.cg AND cc.Code = v.c
);
GO

-- 발주(PurchaseOrder) 상태
INSERT INTO dbo.CommonCodes (CodeGroup, Code, CodeName, SortOrder, IsActive, CreatedAt, CreatedBy)
SELECT v.cg, v.c, v.cn, v.so, 1, SYSUTCDATETIME(), 'system'
FROM (VALUES
    ('PO_STATUS', '0', '초안(Draft)',              1),
    ('PO_STATUS', '1', '발행(Issued)',             2),
    ('PO_STATUS', '2', '부분입고(Partially Received)', 3),
    ('PO_STATUS', '3', '입고완료(Received)',       4),
    ('PO_STATUS', '4', '마감(Closed)',             5)
) v(cg, c, cn, so)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CommonCodes cc WHERE cc.CodeGroup = v.cg AND cc.Code = v.c
);
GO

-- 작업지시(WorkOrder) 상태
INSERT INTO dbo.CommonCodes (CodeGroup, Code, CodeName, SortOrder, IsActive, CreatedAt, CreatedBy)
SELECT v.cg, v.c, v.cn, v.so, 1, SYSUTCDATETIME(), 'system'
FROM (VALUES
    ('WO_STATUS', '0', '계획(Planned)',    1),
    ('WO_STATUS', '1', '릴리즈(Released)', 2),
    ('WO_STATUS', '2', '작업중(In Progress)', 3),
    ('WO_STATUS', '3', '완료(Completed)', 4),
    ('WO_STATUS', '4', '마감(Closed)',    5)
) v(cg, c, cn, so)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CommonCodes cc WHERE cc.CodeGroup = v.cg AND cc.Code = v.c
);
GO

-- LOT 상태
INSERT INTO dbo.CommonCodes (CodeGroup, Code, CodeName, SortOrder, IsActive, CreatedAt, CreatedBy)
SELECT v.cg, v.c, v.cn, v.so, 1, SYSUTCDATETIME(), 'system'
FROM (VALUES
    ('LOT_STATUS', '0', '생성(Created)',       1),
    ('LOT_STATUS', '1', '공정중(In Process)',  2),
    ('LOT_STATUS', '2', '품질보류(QC Hold)',   3),
    ('LOT_STATUS', '3', '출하가능(Released)',  4),
    ('LOT_STATUS', '4', '출하완료(Shipped)',   5),
    ('LOT_STATUS', '5', '폐기(Scrapped)',      6)
) v(cg, c, cn, so)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CommonCodes cc WHERE cc.CodeGroup = v.cg AND cc.Code = v.c
);
GO

-- 출하(Shipment) 상태
INSERT INTO dbo.CommonCodes (CodeGroup, Code, CodeName, SortOrder, IsActive, CreatedAt, CreatedBy)
SELECT v.cg, v.c, v.cn, v.so, 1, SYSUTCDATETIME(), 'system'
FROM (VALUES
    ('SHIP_STATUS', '0', '초안(Draft)',       1),
    ('SHIP_STATUS', '1', '피킹(Picking)',     2),
    ('SHIP_STATUS', '2', '출하완료(Shipped)', 3),
    ('SHIP_STATUS', '3', '배송완료(Delivered)', 4),
    ('SHIP_STATUS', '4', '취소(Cancelled)',   5)
) v(cg, c, cn, so)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CommonCodes cc WHERE cc.CodeGroup = v.cg AND cc.Code = v.c
);
GO
