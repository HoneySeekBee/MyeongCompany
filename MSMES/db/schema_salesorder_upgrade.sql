-- =============================================================
-- SalesOrders 테이블 업그레이드
-- Note 컬럼 추가 + 샘플 데이터 5건
-- =============================================================

-- 1. Note 컬럼 추가
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SalesOrders') AND name = N'Note'
)
    ALTER TABLE dbo.SalesOrders ADD Note NVARCHAR(1000) NULL;
GO

-- 2. 샘플 수주 데이터 (이미 있으면 건너뜀)
-- SO-2026-0001 : 현대자동차 부품
IF NOT EXISTS (SELECT 1 FROM dbo.SalesOrders WHERE SalesOrderNo = N'SO-2026-0001')
BEGIN
    INSERT INTO dbo.SalesOrders
        (SalesOrderNo, CustomerCode, CustomerName, OrderDate, DueDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0001', N'CUS-001', N'현대자동차(주)', '2026-01-10', '2026-02-28', 1 /*Confirmed*/,
         N'1분기 정기 발주 건. 품질 기준서 Ver.3 적용.',
         SYSUTCDATETIME(), N'admin');

    INSERT INTO dbo.SalesOrderItems
        (SalesOrderNo, ItemNo, ItemCode, ItemName, Quantity, UnitPrice, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0001', 1, N'PART-A001', N'엔진 마운트 브라켓', 500, 12500, SYSUTCDATETIME(), N'admin'),
        (N'SO-2026-0001', 2, N'PART-A002', N'서스펜션 링크 조립품', 300, 28000, SYSUTCDATETIME(), N'admin'),
        (N'SO-2026-0001', 3, N'PART-A003', N'스티어링 샤프트', 200, 45000, SYSUTCDATETIME(), N'admin');
END
GO

-- SO-2026-0002 : 삼성전자 케이스
IF NOT EXISTS (SELECT 1 FROM dbo.SalesOrders WHERE SalesOrderNo = N'SO-2026-0002')
BEGIN
    INSERT INTO dbo.SalesOrders
        (SalesOrderNo, CustomerCode, CustomerName, OrderDate, DueDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0002', N'CUS-002', N'삼성전자(주)', '2026-02-05', '2026-03-15', 2 /*InProduction*/,
         N'갤럭시 S시리즈 케이스 금형 변경 요청 포함.',
         SYSUTCDATETIME(), N'admin');

    INSERT INTO dbo.SalesOrderItems
        (SalesOrderNo, ItemNo, ItemCode, ItemName, Quantity, UnitPrice, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0002', 1, N'PART-B001', N'스마트폰 배터리 커버', 2000, 3800, SYSUTCDATETIME(), N'admin'),
        (N'SO-2026-0002', 2, N'PART-B002', N'카메라 모듈 브라켓', 2000, 5200, SYSUTCDATETIME(), N'admin');
END
GO

-- SO-2026-0003 : LG전자 패널
IF NOT EXISTS (SELECT 1 FROM dbo.SalesOrders WHERE SalesOrderNo = N'SO-2026-0003')
BEGIN
    INSERT INTO dbo.SalesOrders
        (SalesOrderNo, CustomerCode, CustomerName, OrderDate, DueDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0003', N'CUS-003', N'LG전자(주)', '2026-02-20', '2026-04-30', 0 /*Draft*/,
         N'신규 고객사 초도 발주. 견적서 검토 후 확정 예정.',
         SYSUTCDATETIME(), N'admin');

    INSERT INTO dbo.SalesOrderItems
        (SalesOrderNo, ItemNo, ItemCode, ItemName, Quantity, UnitPrice, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0003', 1, N'PART-C001', N'OLED 패널 프레임', 400, 67000, SYSUTCDATETIME(), N'admin'),
        (N'SO-2026-0003', 2, N'PART-C002', N'백라이트 유닛 커버', 400, 23000, SYSUTCDATETIME(), N'admin'),
        (N'SO-2026-0003', 3, N'PART-C003', N'PCB 고정 스탠드오프', 1600, 1200, SYSUTCDATETIME(), N'admin');
END
GO

-- SO-2026-0004 : 기아자동차
IF NOT EXISTS (SELECT 1 FROM dbo.SalesOrders WHERE SalesOrderNo = N'SO-2026-0004')
BEGIN
    INSERT INTO dbo.SalesOrders
        (SalesOrderNo, CustomerCode, CustomerName, OrderDate, DueDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0004', N'CUS-004', N'기아자동차(주)', '2026-03-03', '2026-05-10', 3 /*Shipped*/,
         N'EV6 모델용 부품. 출하 완료. 최종 검수 확인 요망.',
         SYSUTCDATETIME(), N'admin');

    INSERT INTO dbo.SalesOrderItems
        (SalesOrderNo, ItemNo, ItemCode, ItemName, Quantity, UnitPrice, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0004', 1, N'PART-D001', N'전기차 배터리 트레이', 150, 185000, SYSUTCDATETIME(), N'admin'),
        (N'SO-2026-0004', 2, N'PART-D002', N'충전 포트 커버 어셈블리', 150, 42000, SYSUTCDATETIME(), N'admin');
END
GO

-- SO-2026-0005 : 포스코 설비
IF NOT EXISTS (SELECT 1 FROM dbo.SalesOrders WHERE SalesOrderNo = N'SO-2026-0005')
BEGIN
    INSERT INTO dbo.SalesOrders
        (SalesOrderNo, CustomerCode, CustomerName, OrderDate, DueDate, Status, Note, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0005', N'CUS-005', N'포스코(주)', '2026-04-01', '2026-06-30', 4 /*Closed*/,
         N'설비 보수용 교체 부품 공급 완료. 마감 처리.',
         SYSUTCDATETIME(), N'admin');

    INSERT INTO dbo.SalesOrderItems
        (SalesOrderNo, ItemNo, ItemCode, ItemName, Quantity, UnitPrice, CreatedAt, CreatedBy)
    VALUES
        (N'SO-2026-0005', 1, N'PART-E001', N'압연기 롤러 베어링', 80, 320000, SYSUTCDATETIME(), N'admin'),
        (N'SO-2026-0005', 2, N'PART-E002', N'컨베이어 체인 링크', 500, 18000, SYSUTCDATETIME(), N'admin'),
        (N'SO-2026-0005', 3, N'PART-E003', N'유압 실린더 실링 키트', 200, 56000, SYSUTCDATETIME(), N'admin');
END
GO

-- 3. NumberSequences 시드 (SO 키가 없으면 추가)
IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE SequenceKey = N'SO')
    INSERT INTO dbo.NumberSequences (SequenceKey, CurrentValue) VALUES (N'SO', 5);
GO
