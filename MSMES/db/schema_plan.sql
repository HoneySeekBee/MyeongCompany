IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductionPlans')
CREATE TABLE ProductionPlans (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    PlanNo       NVARCHAR(50)  NOT NULL UNIQUE,
    WorkOrderNo  NVARCHAR(50)  NULL,
    ProductCode  NVARCHAR(100) NOT NULL,
    ProductName  NVARCHAR(200) NOT NULL,
    PlannedQty   DECIMAL(18,2) NOT NULL,
    StartDate    DATE          NOT NULL,
    EndDate      DATE          NOT NULL,
    Line         NVARCHAR(100) NULL,        -- 생산 라인
    Status       TINYINT       NOT NULL DEFAULT 0,  -- 0:계획, 1:진행중, 2:완료, 3:취소
    Priority     TINYINT       NOT NULL DEFAULT 1,  -- 1:보통, 2:높음, 3:긴급
    Remark       NVARCHAR(500) NULL,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy    NVARCHAR(100) NULL
);
GO

-- Sample plans: 2~3 weeks range centered on today
DECLARE @today DATE = CAST(GETDATE() AS DATE);
INSERT INTO ProductionPlans (PlanNo, ProductCode, ProductName, PlannedQty, StartDate, EndDate, Line, Status, Priority, CreatedBy)
VALUES
  (N'PP-2025-001', N'P-1001', N'전동 드릴 A형',  500, DATEADD(DAY,-5,@today), DATEADD(DAY, 3,@today), N'A라인', 1, 2, N'admin'),
  (N'PP-2025-002', N'P-1002', N'전동 드릴 B형',  300, DATEADD(DAY,-2,@today), DATEADD(DAY, 6,@today), N'B라인', 0, 1, N'admin'),
  (N'PP-2025-003', N'P-2001', N'컴프레서 기본형', 120, DATEADD(DAY, 1,@today), DATEADD(DAY,10,@today), N'C라인', 0, 3, N'admin'),
  (N'PP-2025-004', N'P-1003', N'앵글 그라인더',   200, DATEADD(DAY, 4,@today), DATEADD(DAY,12,@today), N'A라인', 0, 1, N'admin'),
  (N'PP-2025-005', N'P-3001', N'산업용 팬',       80,  DATEADD(DAY,-8,@today), DATEADD(DAY,-1,@today), N'B라인', 2, 1, N'admin');
GO
