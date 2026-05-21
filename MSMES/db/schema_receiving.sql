IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoodsReceipts')
CREATE TABLE GoodsReceipts (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptNo       NVARCHAR(50)  NOT NULL UNIQUE,
    PurchaseOrderNo NVARCHAR(50)  NOT NULL,
    SupplierName    NVARCHAR(200) NOT NULL,
    ReceiptDate     DATE          NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    Status          TINYINT       NOT NULL DEFAULT 0, -- 0:입고대기, 1:검사중, 2:합격, 3:불합격, 4:조건부합격
    InspectorName   NVARCHAR(100) NULL,
    InspectedAt     DATETIME2     NULL,
    Remark          NVARCHAR(500) NULL,
    CreatedAt       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy       NVARCHAR(100) NULL
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoodsReceiptItems')
CREATE TABLE GoodsReceiptItems (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    GoodsReceiptId  INT            NOT NULL REFERENCES GoodsReceipts(Id),
    MaterialCode    NVARCHAR(100)  NOT NULL,
    MaterialName    NVARCHAR(200)  NOT NULL,
    OrderedQty      DECIMAL(18,2)  NOT NULL,
    ReceivedQty     DECIMAL(18,2)  NOT NULL,
    InspectedQty    DECIMAL(18,2)  NOT NULL DEFAULT 0,
    AcceptedQty     DECIMAL(18,2)  NOT NULL DEFAULT 0,
    RejectedQty     DECIMAL(18,2)  NOT NULL DEFAULT 0,
    DefectReason    NVARCHAR(200)  NULL,
    Unit            NVARCHAR(20)   NOT NULL DEFAULT N'EA'
);
GO

-- Sample data
DECLARE @today DATE = CAST(GETDATE() AS DATE);
INSERT INTO GoodsReceipts (ReceiptNo, PurchaseOrderNo, SupplierName, ReceiptDate, Status, InspectorName, InspectedAt, CreatedBy)
VALUES
  (N'GR-2025-001', N'PO-2025-001', N'한국부품(주)', DATEADD(DAY,-3,@today), 2, N'김검사', DATEADD(DAY,-2,GETUTCDATE()), N'admin'),
  (N'GR-2025-002', N'PO-2025-002', N'대성산업',     DATEADD(DAY,-1,@today), 1, NULL, NULL, N'admin'),
  (N'GR-2025-003', N'PO-2025-003', N'신성전자',     @today,                  0, NULL, NULL, N'admin');
GO

DECLARE @id1 INT = (SELECT Id FROM GoodsReceipts WHERE ReceiptNo = N'GR-2025-001');
DECLARE @id2 INT = (SELECT Id FROM GoodsReceipts WHERE ReceiptNo = N'GR-2025-002');
DECLARE @id3 INT = (SELECT Id FROM GoodsReceipts WHERE ReceiptNo = N'GR-2025-003');
INSERT INTO GoodsReceiptItems (GoodsReceiptId, MaterialCode, MaterialName, OrderedQty, ReceivedQty, InspectedQty, AcceptedQty, RejectedQty)
VALUES
  (@id1, N'M-0011', N'모터 250W',    100, 100, 100, 98, 2),
  (@id1, N'M-0041', N'볼트 M6',      500, 500, 500, 500, 0),
  (@id2, N'M-0012', N'기어박스',     50,  50,  20,  18,  2),
  (@id3, N'M-0021', N'하우징 플라스틱', 200, 190, 0, 0, 0);
GO
