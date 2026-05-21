IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SpcMeasurements')
CREATE TABLE SpcMeasurements (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    ProcessCode   NVARCHAR(100) NOT NULL,
    ProcessName   NVARCHAR(200) NOT NULL,
    CharName      NVARCHAR(200) NOT NULL,  -- 측정 특성명 (예: 직경, 두께, 무게)
    NominalValue  DECIMAL(18,6) NOT NULL,  -- 기준치
    UpperSpec     DECIMAL(18,6) NOT NULL,  -- 상한 규격 (USL)
    LowerSpec     DECIMAL(18,6) NOT NULL,  -- 하한 규격 (LSL)
    MeasuredValue DECIMAL(18,6) NOT NULL,  -- 측정값
    SubgroupNo    INT           NOT NULL,  -- 부분군 번호
    SampleNo      INT           NOT NULL,  -- 부분군 내 샘플 번호 (1~5)
    MeasuredBy    NVARCHAR(100) NULL,
    MeasuredAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    LotNo         NVARCHAR(100) NULL
);
GO
CREATE INDEX IX_SpcMeasurements_Process ON SpcMeasurements(ProcessCode, MeasuredAt DESC);
GO

-- Sample data: 25 subgroups × 5 samples for a drilling process
DECLARE @base DECIMAL(18,6) = 10.000;  -- nominal 10mm diameter
DECLARE @usl  DECIMAL(18,6) = 10.050;
DECLARE @lsl  DECIMAL(18,6) = 9.950;
DECLARE @i INT = 1;
DECLARE @j INT;
WHILE @i <= 25
BEGIN
    SET @j = 1;
    WHILE @j <= 5
    BEGIN
        DECLARE @noise DECIMAL(18,6) = (CAST(CHECKSUM(NEWID()) AS FLOAT) / 2147483647.0) * 0.030;
        -- Introduce a process shift at subgroup 18 to make it interesting
        DECLARE @shift DECIMAL(18,6) = CASE WHEN @i >= 18 THEN 0.015 ELSE 0.000 END;
        INSERT INTO SpcMeasurements (ProcessCode, ProcessName, CharName, NominalValue, UpperSpec, LowerSpec,
            MeasuredValue, SubgroupNo, SampleNo, MeasuredBy, MeasuredAt)
        VALUES (N'P-DRILL-01', N'드릴링 공정', N'구멍 직경 (mm)',
            @base, @usl, @lsl,
            @base + @shift + @noise,
            @i, @j, N'홍길동',
            DATEADD(HOUR, -(@i * 2), GETUTCDATE()));
        SET @j = @j + 1;
    END
    SET @i = @i + 1;
END
GO
