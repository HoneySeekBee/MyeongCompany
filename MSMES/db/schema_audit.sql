IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
CREATE TABLE AuditLogs (
    Id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId      NVARCHAR(100) NOT NULL,
    UserName    NVARCHAR(200) NOT NULL,
    Action      NVARCHAR(100) NOT NULL,   -- CREATE, UPDATE, DELETE, LOGIN, LOGOUT
    EntityType  NVARCHAR(100) NOT NULL,   -- SalesOrder, PurchaseOrder, Lot, etc.
    EntityId    NVARCHAR(200) NULL,
    Description NVARCHAR(1000) NULL,
    IpAddress   NVARCHAR(50) NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_AuditLogs_UserId   ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt DESC);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
