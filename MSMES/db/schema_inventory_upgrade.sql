-- ============================================================
-- Inventory upgrade: safety stock levels, warehouse locations,
-- max stock, and real-time shortage detection support
-- Target DB  : MSMES  (localdb)\MSSQLLocalDB
-- Table      : dbo.Inventories
-- ============================================================

USE MSMES;
GO

-- 1. MaxStock -----------------------------------------------
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Inventories') AND name = N'MaxStock'
)
BEGIN
    ALTER TABLE dbo.Inventories
        ADD MaxStock DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inventories_MaxStock DEFAULT 0;
    PRINT N'Column MaxStock added.';
END
ELSE
    PRINT N'Column MaxStock already exists — skipped.';
GO

-- 2. WarehouseLocation --------------------------------------
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Inventories') AND name = N'WarehouseLocation'
)
BEGIN
    ALTER TABLE dbo.Inventories
        ADD WarehouseLocation NVARCHAR(50) NULL;
    PRINT N'Column WarehouseLocation added.';
END
ELSE
    PRINT N'Column WarehouseLocation already exists — skipped.';
GO

-- 3. SafetyStock — already exists in original schema, guard anyway
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Inventories') AND name = N'SafetyStock'
)
BEGIN
    ALTER TABLE dbo.Inventories
        ADD SafetyStock DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inventories_SafetyStock DEFAULT 0;
    PRINT N'Column SafetyStock added.';
END
ELSE
    PRINT N'Column SafetyStock already exists — skipped.';
GO

-- 4. Unit — already exists in original schema, guard anyway
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Inventories') AND name = N'Unit'
)
BEGIN
    ALTER TABLE dbo.Inventories
        ADD Unit NVARCHAR(20) NOT NULL CONSTRAINT DF_Inventories_Unit DEFAULT N'EA';
    PRINT N'Column Unit added.';
END
ELSE
    PRINT N'Column Unit already exists — skipped.';
GO

-- 5. Seed MaxStock and WarehouseLocation for rows that have no values
--    MaxStock   = 2× CurrentStock (floor 1 to avoid divide-by-zero)
--    WarehouseLocation = WarehouseCode + '-' + zero-padded Id rack
UPDATE dbo.Inventories
SET
    MaxStock = CASE
                   WHEN MaxStock = 0 AND CurrentStock > 0
                   THEN CurrentStock * 2
                   WHEN MaxStock = 0
                   THEN SafetyStock  * 2
                   ELSE MaxStock
               END,
    WarehouseLocation = CASE
                            WHEN WarehouseLocation IS NULL OR WarehouseLocation = N''
                            THEN WarehouseCode + N'-' + RIGHT(N'00' + CAST(Id AS NVARCHAR(10)), 3)
                            ELSE WarehouseLocation
                        END
WHERE MaxStock = 0 OR WarehouseLocation IS NULL OR WarehouseLocation = N'';
GO

PRINT N'Inventory upgrade complete.';
GO
