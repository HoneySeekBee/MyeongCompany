IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OeeRecords')
CREATE TABLE OeeRecords (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    EquipmentId         INT NOT NULL,
    EquipmentName       NVARCHAR(200) NOT NULL,
    RecordDate          DATE NOT NULL,
    PlannedTimeMinutes  INT NOT NULL,
    ActualTimeMinutes   INT NOT NULL,
    IdealCyclePerMinute DECIMAL(10,4) NOT NULL,
    ActualProduced      DECIMAL(18,2) NOT NULL,
    GoodQuantity        DECIMAL(18,2) NOT NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Sample OEE data: 7 days x 3 equipment
DECLARE @i INT = 6;
WHILE @i >= 0
BEGIN
    DECLARE @d DATE = DATEADD(DAY, -@i, CAST(GETDATE() AS DATE));
    DECLARE @at1 INT = 420 + (@i * 5 % 30);
    DECLARE @at2 INT = 450 + (@i * 3 % 20);
    DECLARE @at3 INT = 400 + (@i * 8 % 40);
    DECLARE @ap1 DECIMAL(18,2) = 900  + @i * 20;
    DECLARE @ap2 DECIMAL(18,2) = 1200 + @i * 15;
    DECLARE @ap3 DECIMAL(18,2) = 680  + @i * 12;
    DECLARE @gq1 DECIMAL(18,2) = 870  + @i * 18;
    DECLARE @gq2 DECIMAL(18,2) = 1180 + @i * 14;
    DECLARE @gq3 DECIMAL(18,2) = 650  + @i * 11;

    INSERT INTO OeeRecords (EquipmentId, EquipmentName, RecordDate, PlannedTimeMinutes, ActualTimeMinutes, IdealCyclePerMinute, ActualProduced, GoodQuantity)
    VALUES (1, N'사출성형기 #1', @d, 480, @at1, 2.5, @ap1, @gq1);

    INSERT INTO OeeRecords (EquipmentId, EquipmentName, RecordDate, PlannedTimeMinutes, ActualTimeMinutes, IdealCyclePerMinute, ActualProduced, GoodQuantity)
    VALUES (2, N'조립라인 #1', @d, 480, @at2, 3.0, @ap2, @gq2);

    INSERT INTO OeeRecords (EquipmentId, EquipmentName, RecordDate, PlannedTimeMinutes, ActualTimeMinutes, IdealCyclePerMinute, ActualProduced, GoodQuantity)
    VALUES (3, N'CNC 머시닝센터', @d, 480, @at3, 1.8, @ap3, @gq3);

    SET @i = @i - 1;
END
GO
