-- =============================================
-- upgrade_010: Physical-delete audit triggers (one per entity)
-- -------------------------------------------------------------
-- Replaces the former single trg_Permission_Global audit trigger.
--   * trg_Permission_Global is DROPPED (it audited INSERT/UPDATE/DELETE on
--     Permission only via SESSION_CONTEXT).
--   * Audit of INSERT/UPDATE now happens in C# (SaveChangesAsync) for ALL
--     entities with exact origin (Web/API/Auto).
--   * These AFTER DELETE-only triggers audit physical deletes for every
--     auditable entity, catching deletes done both from the app and by direct
--     SQL (which C# can never see).
-- Idempotent: safe to re-run. Auditable set = all entities except AuditLog
-- (self-audits via its own CreatedAt/CreatedByUserId) and DatabaseTable
-- (infrastructure of the audit mechanism itself).
-- =============================================

-- 1. Ensure ShiftType is registered in DatabaseTable (missing previously;
--    trg_ShiftType_UpdateModifiedAt resolved a NULL TableId -> runtime error).
IF NOT EXISTS (SELECT 1 FROM [DatabaseTable] WHERE Description = 'ShiftType')
BEGIN
    INSERT INTO [DatabaseTable] (Description, CreatedAt, Status, CreatedByUserId)
    VALUES ('ShiftType', GETDATE(), 1, 1);
END
GO

-- 2. Remove the legacy global audit trigger (replaced by C#-side INSERT/UPDATE audit).
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Permission_Audit') DROP TRIGGER trg_Permission_Audit;
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Permission_UpdateModifiedAt') DROP TRIGGER trg_Permission_UpdateModifiedAt;
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Permission_Global') DROP TRIGGER trg_Permission_Global;
GO

-- 3. Physical-delete audit triggers (one per auditable entity).

CREATE OR ALTER TRIGGER trg_ApiFunction_AuditDelete ON [ApiFunction] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'ApiFunction');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_ApiKey_AuditDelete ON [ApiKey] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'ApiKey');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_ApiPermission_AuditDelete ON [ApiPermission] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'ApiPermission');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_ApiKeyTablePermission_AuditDelete ON [ApiKeyTablePermission] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'ApiKeyTablePermission');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_Area_AuditDelete ON [Area] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'Area');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_Cutoff_AuditDelete ON [Cutoff] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'Cutoff');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_Department_AuditDelete ON [Department] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'Department');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_Permission_AuditDelete ON [Permission] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'Permission');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_Position_AuditDelete ON [Position] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'Position');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_PermissionType_AuditDelete ON [PermissionType] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'PermissionType');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_UserType_AuditDelete ON [UserType] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'UserType');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_User_AuditDelete ON [User] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'User');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_ResetPasswordToken_AuditDelete ON [ResetPasswordToken] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'ResetPasswordToken');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_OriginType_AuditDelete ON [OriginType] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'OriginType');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_CutoffStatusType_AuditDelete ON [CutoffStatusType] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'CutoffStatusType');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_PermissionStatusType_AuditDelete ON [PermissionStatusType] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'PermissionStatusType');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO

CREATE OR ALTER TRIGGER trg_ShiftType_AuditDelete ON [ShiftType] AFTER DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'ShiftType');
    INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
    SELECT @TableId, d.Id, 4,
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3),
           (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1),
           GETDATE(), 1
    FROM deleted d;
END;
GO
