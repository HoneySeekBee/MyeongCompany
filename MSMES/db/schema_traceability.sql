-- Link table: which materials went into which LOT
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LotMaterials')
CREATE TABLE LotMaterials (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    LotNo        NVARCHAR(100) NOT NULL,
    MaterialCode NVARCHAR(100) NOT NULL,
    MaterialName NVARCHAR(200) NOT NULL,
    Quantity     DECIMAL(18,2) NOT NULL,
    Unit         NVARCHAR(20)  NOT NULL DEFAULT N'EA',
    ReceiptNo    NVARCHAR(100) NULL,   -- 수입검사 입고번호
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
GO
CREATE INDEX IX_LotMaterials_LotNo ON LotMaterials(LotNo);
CREATE INDEX IX_LotMaterials_MaterialCode ON LotMaterials(MaterialCode);
GO

-- Sample: link existing lots to materials
DECLARE @lot1 NVARCHAR(100) = (SELECT TOP 1 LotNo FROM Lots ORDER BY CreatedAt DESC);
DECLARE @lot2 NVARCHAR(100) = (SELECT TOP 1 LotNo FROM Lots ORDER BY CreatedAt);

IF @lot1 IS NOT NULL
BEGIN
    INSERT INTO LotMaterials (LotNo, MaterialCode, MaterialName, Quantity, Unit, ReceiptNo)
    VALUES
      (@lot1, N'M-0011', N'모터 250W',     1,   N'EA', N'GR-2025-001'),
      (@lot1, N'M-0012', N'기어박스',       1,   N'EA', N'GR-2025-001'),
      (@lot1, N'M-0041', N'볼트 M6',        12,  N'EA', N'GR-2025-001'),
      (@lot1, N'M-0021', N'하우징 플라스틱', 2,   N'EA', N'GR-2025-003');
END
IF @lot2 IS NOT NULL AND @lot2 <> @lot1
BEGIN
    INSERT INTO LotMaterials (LotNo, MaterialCode, MaterialName, Quantity, Unit)
    VALUES
      (@lot2, N'M-0051', N'압축 실린더',    1, N'EA'),
      (@lot2, N'M-0052', N'모터 1.5kW',    1, N'EA'),
      (@lot2, N'M-0053', N'압력 탱크 50L', 1, N'EA');
END
GO
