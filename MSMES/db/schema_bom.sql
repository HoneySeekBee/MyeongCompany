IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Boms')
CREATE TABLE Boms (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    BomNo       NVARCHAR(50)  NOT NULL UNIQUE,
    ProductCode NVARCHAR(100) NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    Version     NVARCHAR(20)  NOT NULL DEFAULT N'1.0',
    IsActive    BIT           NOT NULL DEFAULT 1,
    Remark      NVARCHAR(500) NULL,
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy   NVARCHAR(100) NULL
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BomItems')
CREATE TABLE BomItems (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    BomId         INT            NOT NULL REFERENCES Boms(Id),
    ItemNo        INT            NOT NULL,          -- 순번
    MaterialCode  NVARCHAR(100)  NOT NULL,
    MaterialName  NVARCHAR(200)  NOT NULL,
    Quantity      DECIMAL(18,4)  NOT NULL,
    Unit          NVARCHAR(20)   NOT NULL DEFAULT N'EA',
    Remark        NVARCHAR(200)  NULL
);
GO

-- Sample data
INSERT INTO Boms (BomNo, ProductCode, ProductName, Version, IsActive, CreatedBy)
VALUES
  (N'BOM-2025-001', N'P-1001', N'전동 드릴 A형', N'1.0', 1, N'admin'),
  (N'BOM-2025-002', N'P-1002', N'전동 드릴 B형', N'1.0', 1, N'admin'),
  (N'BOM-2025-003', N'P-2001', N'컴프레서 기본형', N'2.1', 1, N'admin');
GO

DECLARE @id1 INT = (SELECT Id FROM Boms WHERE BomNo = N'BOM-2025-001');
DECLARE @id2 INT = (SELECT Id FROM Boms WHERE BomNo = N'BOM-2025-002');
DECLARE @id3 INT = (SELECT Id FROM Boms WHERE BomNo = N'BOM-2025-003');

INSERT INTO BomItems (BomId, ItemNo, MaterialCode, MaterialName, Quantity, Unit)
VALUES
  (@id1, 1, N'M-0011', N'모터 (250W)',     1, N'EA'),
  (@id1, 2, N'M-0012', N'기어박스',         1, N'EA'),
  (@id1, 3, N'M-0021', N'하우징 (플라스틱)',2, N'EA'),
  (@id1, 4, N'M-0031', N'전원 케이블',      1.5, N'M'),
  (@id1, 5, N'M-0041', N'볼트 M6',         12, N'EA'),
  (@id2, 1, N'M-0011', N'모터 (400W)',      1, N'EA'),
  (@id2, 2, N'M-0012', N'기어박스 (고속)',  1, N'EA'),
  (@id2, 3, N'M-0022', N'하우징 (알루미늄)',2, N'EA'),
  (@id2, 4, N'M-0031', N'전원 케이블',      1.5, N'M'),
  (@id3, 1, N'M-0051', N'압축 실린더',      1, N'EA'),
  (@id3, 2, N'M-0052', N'모터 (1.5kW)',     1, N'EA'),
  (@id3, 3, N'M-0053', N'압력 탱크 (50L)', 1, N'EA'),
  (@id3, 4, N'M-0054', N'압력 게이지',      2, N'EA');
GO
