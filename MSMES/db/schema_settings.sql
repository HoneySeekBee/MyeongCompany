-- ============================================================
-- SystemSettings 테이블 생성 및 초기 데이터 시드
-- Run against: (localdb)\MSSQLLocalDB / MSMES
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'dbo' AND t.name = 'SystemSettings'
)
BEGIN
    CREATE TABLE dbo.SystemSettings
    (
        SettingKey   NVARCHAR(100)  NOT NULL,
        SettingValue NVARCHAR(2000) NULL,
        Description  NVARCHAR(500)  NULL,
        UpdatedAt    DATETIME2      NULL,
        UpdatedBy    NVARCHAR(100)  NULL,

        CONSTRAINT PK_SystemSettings PRIMARY KEY CLUSTERED (SettingKey)
    );

    PRINT 'dbo.SystemSettings 테이블이 생성되었습니다.';
END
ELSE
BEGIN
    PRINT 'dbo.SystemSettings 테이블이 이미 존재합니다.';
END
GO

-- ── 회사 정보 ──────────────────────────────────────────────
INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'Company.Name', N'(주)명성산업', N'회사명', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'Company.Name');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'Company.BusinessNo', N'123-45-67890', N'사업자등록번호', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'Company.BusinessNo');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'Company.CEO', N'홍길동', N'대표이사', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'Company.CEO');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'Company.Phone', N'02-1234-5678', N'대표전화', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'Company.Phone');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'Company.Address', N'서울특별시 강남구 테헤란로 123', N'본사 주소', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'Company.Address');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'Company.Email', N'contact@msmes.co.kr', N'대표 이메일', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'Company.Email');

-- ── 시스템 설정 ────────────────────────────────────────────
INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'System.LowStockThreshold', N'10', N'재고 부족 경고 임계값 (수량)', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'System.LowStockThreshold');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'System.AlertEmailEnabled', N'false', N'경보 이메일 발송 활성화 여부', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'System.AlertEmailEnabled');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'System.SessionTimeoutMinutes', N'480', N'세션 타임아웃 (분)', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'System.SessionTimeoutMinutes');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'System.DefaultWorkOrderPrefix', N'WO', N'작업지시 번호 접두어', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'System.DefaultWorkOrderPrefix');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'System.DefaultSalesOrderPrefix', N'SO', N'수주 번호 접두어', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'System.DefaultSalesOrderPrefix');

INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description, UpdatedAt, UpdatedBy)
SELECT N'System.DefaultPurchaseOrderPrefix', N'PO', N'발주 번호 접두어', SYSUTCDATETIME(), N'SYSTEM'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = N'System.DefaultPurchaseOrderPrefix');

GO
PRINT '시드 데이터 삽입 완료.';
