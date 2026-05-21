IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Alerts')
CREATE TABLE Alerts (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    AlertType   NVARCHAR(50)  NOT NULL,  -- STOCK_LOW, EQUIP_FAULT, DELIVERY_DUE, QUALITY, SYSTEM
    Severity    TINYINT       NOT NULL DEFAULT 1, -- 1:정보, 2:경고, 3:위험
    Title       NVARCHAR(200) NOT NULL,
    Message     NVARCHAR(1000) NOT NULL,
    EntityType  NVARCHAR(100) NULL,
    EntityId    NVARCHAR(200) NULL,
    IsRead      BIT           NOT NULL DEFAULT 0,
    IsResolved  BIT           NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Sample alerts
INSERT INTO Alerts (AlertType, Severity, Title, Message, EntityType, EntityId, IsRead, IsResolved)
VALUES
  (N'STOCK_LOW',     3, N'재고 부족 위험', N'볼트 M6 (M-0041) 재고가 안전재고 이하입니다. 현재 재고: 50EA, 안전재고: 200EA', N'Inventory', N'M-0041', 0, 0),
  (N'EQUIP_FAULT',   3, N'설비 고장 발생', N'사출성형기 #1 (EQ-001) 이상 감지. 즉시 점검이 필요합니다.', N'Equipment', N'EQ-001', 0, 0),
  (N'DELIVERY_DUE',  2, N'납기 임박', N'수주 SO-2025-003 (삼성전자) 납기일이 2일 후입니다. 현재 생산진행률 60%.', N'SalesOrder', N'SO-2025-003', 0, 0),
  (N'QUALITY',       2, N'품질 이상', N'LOT-2025-089 불량률 8.5% 초과 감지. 기준치: 3%', N'Lot', N'LOT-2025-089', 0, 0),
  (N'STOCK_LOW',     2, N'재고 부족 경고', N'모터 250W (M-0011) 재고가 적정 수준 이하입니다. 발주를 검토하세요.', N'Inventory', N'M-0011', 1, 0),
  (N'SYSTEM',        1, N'정기 백업 완료', N'데이터베이스 정기 백업이 완료되었습니다.', NULL, NULL, 1, 1);
GO
