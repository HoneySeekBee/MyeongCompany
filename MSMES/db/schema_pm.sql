-- PM Schedule table (예방정비 계획)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PmSchedules')
CREATE TABLE PmSchedules (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    EquipmentId     INT            NOT NULL,
    EquipmentName   NVARCHAR(200)  NOT NULL,
    PmType          NVARCHAR(100)  NOT NULL,   -- 일상점검, 주간점검, 월간점검, 연간정비
    IntervalDays    INT            NOT NULL,   -- 주기(일)
    LastPmDate      DATE           NULL,
    NextPmDate      DATE           NOT NULL,
    AssignedTo      NVARCHAR(100)  NULL,
    CheckItems      NVARCHAR(1000) NULL,       -- 점검 항목 (줄바꿈 구분)
    IsActive        BIT            NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PmRecords')
CREATE TABLE PmRecords (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PmScheduleId    INT            NOT NULL REFERENCES PmSchedules(Id),
    EquipmentId     INT            NOT NULL,
    EquipmentName   NVARCHAR(200)  NOT NULL,
    PmDate          DATE           NOT NULL,
    Technician      NVARCHAR(100)  NOT NULL,
    Result          TINYINT        NOT NULL DEFAULT 0,  -- 0:정상, 1:이상발견, 2:수리완료
    FindingsNote    NVARCHAR(1000) NULL,
    NextPmDate      DATE           NULL,
    WorkTimeMinutes INT            NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Calculate MTBF/MTTR view
IF OBJECT_ID('vw_EquipmentReliability', 'V') IS NOT NULL DROP VIEW vw_EquipmentReliability;
GO
CREATE VIEW vw_EquipmentReliability AS
SELECT
    EquipmentId,
    EquipmentName,
    COUNT(*) AS TotalPmCount,
    SUM(CASE WHEN Result = 1 THEN 1 ELSE 0 END) AS AnomalyCount,
    AVG(CAST(WorkTimeMinutes AS FLOAT)) AS AvgWorkMinutes
FROM PmRecords
GROUP BY EquipmentId, EquipmentName;
GO

-- Sample PM Schedules
DECLARE @today DATE = CAST(GETDATE() AS DATE);
INSERT INTO PmSchedules (EquipmentId, EquipmentName, PmType, IntervalDays, LastPmDate, NextPmDate, AssignedTo, CheckItems)
VALUES
  (1, N'사출성형기 #1', N'일상점검', 1,  DATEADD(DAY,-1,@today), @today,                N'김기술', N'오일레벨 확인\n냉각수 점검\n소음/진동 이상유무'),
  (1, N'사출성형기 #1', N'월간점검', 30, DATEADD(DAY,-15,@today), DATEADD(DAY,15,@today), N'이정비', N'필터 청소\n벨트 장력 점검\n전기 접속부 점검\n안전장치 작동확인'),
  (2, N'조립라인 #1',  N'주간점검', 7,  DATEADD(DAY,-3,@today), DATEADD(DAY,4,@today),  N'박기사', N'컨베이어 체인 윤활\n센서 동작 확인\n비상정지 테스트'),
  (3, N'CNC 머시닝센터',N'월간점검', 30, DATEADD(DAY,-28,@today),DATEADD(DAY,2,@today),  N'최기술', N'스핀들 베어링 점검\n절삭유 보충\n공구 마모 확인\n정밀도 측정'),
  (3, N'CNC 머시닝센터',N'연간정비', 365,DATEADD(DAY,-200,@today),DATEADD(DAY,165,@today),N'외주업체', N'전체 오버홀\n베어링 교체\n가이드웨이 점검\n전기 배선 점검');
GO

-- Sample PM Records (completed maintenance history)
INSERT INTO PmRecords (PmScheduleId, EquipmentId, EquipmentName, PmDate, Technician, Result, FindingsNote, WorkTimeMinutes)
VALUES
  (1, 1, N'사출성형기 #1', DATEADD(DAY,-1,CAST(GETDATE() AS DATE)), N'김기술', 0, N'이상없음', 30),
  (1, 1, N'사출성형기 #1', DATEADD(DAY,-2,CAST(GETDATE() AS DATE)), N'김기술', 1, N'냉각수 소량 누수 발견, 호스 교체', 90),
  (3, 2, N'조립라인 #1',  DATEADD(DAY,-3,CAST(GETDATE() AS DATE)), N'박기사', 0, N'정상', 45),
  (4, 3, N'CNC 머시닝센터',DATEADD(DAY,-28,CAST(GETDATE() AS DATE)),N'최기술', 2, N'스핀들 베어링 마모 발견, 교체 완료', 240);
GO
