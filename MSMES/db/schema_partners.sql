IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Partners')
CREATE TABLE Partners (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    PartnerCode  NVARCHAR(50)  NOT NULL UNIQUE,
    PartnerName  NVARCHAR(200) NOT NULL,
    PartnerType  NVARCHAR(20)  NOT NULL,  -- CUSTOMER, SUPPLIER, BOTH
    BusinessNo   NVARCHAR(20)  NULL,      -- 사업자번호
    RepName      NVARCHAR(100) NULL,      -- 대표자명
    Tel          NVARCHAR(50)  NULL,
    Email        NVARCHAR(200) NULL,
    Address      NVARCHAR(500) NULL,
    ContactName  NVARCHAR(100) NULL,      -- 담당자
    ContactTel   NVARCHAR(50)  NULL,
    PaymentTerms NVARCHAR(100) NULL,      -- 결제조건 (예: 월말정산 60일)
    CreditLimit  DECIMAL(18,0) NULL,      -- 신용한도
    Rating       TINYINT       NOT NULL DEFAULT 3, -- 1~5 등급
    IsActive     BIT           NOT NULL DEFAULT 1,
    Memo         NVARCHAR(1000) NULL,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy    NVARCHAR(100) NULL
);
GO

-- Sample partners
INSERT INTO Partners (PartnerCode, PartnerName, PartnerType, BusinessNo, RepName, Tel, Email, ContactName, ContactTel, PaymentTerms, CreditLimit, Rating, CreatedBy)
VALUES
  (N'C-001', N'삼성전자(주)',     N'CUSTOMER', N'124-81-00998', N'한종희',   N'031-200-1114', N'sales@samsung.com',    N'김영업',   N'031-200-2345', N'월말 60일',  500000000, 5, N'admin'),
  (N'C-002', N'LG전자(주)',       N'CUSTOMER', N'107-86-14075', N'조주완',   N'02-3777-1114', N'contact@lge.com',      N'박담당',   N'02-3777-2345', N'월말 30일',  300000000, 4, N'admin'),
  (N'C-003', N'현대자동차(주)',   N'CUSTOMER', N'120-81-00221', N'장재훈',   N'02-3464-1114', N'purchase@hyundai.com', N'이구매',   N'02-3464-2345', N'익월 45일',  200000000, 5, N'admin'),
  (N'S-001', N'한국부품(주)',     N'SUPPLIER', N'220-81-12345', N'김부품',   N'032-123-4567', N'sales@kr-parts.com',   N'최납품',   N'032-123-5678', N'현금 즉시',  NULL,      3, N'admin'),
  (N'S-002', N'대성산업',         N'SUPPLIER', N'312-86-54321', N'이대성',   N'051-234-5678', N'order@daesung.co.kr',  N'박자재',   N'051-234-6789', N'월말 30일',  NULL,      4, N'admin'),
  (N'S-003', N'신성전자',         N'SUPPLIER', N'401-81-11111', N'박신성',   N'031-345-6789', N'buy@sinsung.co.kr',    N'김전자',   N'031-345-7890', N'30일 후불',  NULL,      3, N'admin'),
  (N'B-001', N'동방무역(주)',     N'BOTH',     N'130-81-33333', N'정동방',   N'02-456-7890',  N'trade@dongbang.com',   N'오무역',   N'02-456-8901', N'협의',       100000000, 3, N'admin');
GO
