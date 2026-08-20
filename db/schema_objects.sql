-- =============================================
-- TABLE TYPES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'IdTableType' AND is_table_type = 1)
BEGIN
    CREATE TYPE IdTableType AS TABLE (Id INT);
END;
GO

-- =============================================
-- 1. LoginUser
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[LoginUser]
    @email NVARCHAR(100), @password NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.Id, u.Name, u.Email, u.AreaId, a.Description AS AreaName, u.UserTypeId
    FROM [User] u INNER JOIN Area a ON u.AreaId = a.Id
    WHERE u.Email = @email AND u.PasswordHash = @password;
END;
GO

-- =============================================
-- 2. GetPermissionsAuthorizer
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetPermissionsAuthorizer]
    @AreaId INT, @Status BIT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Reason AS Description, p.Days AS RequestedDays, p.WithPay AS WithPay, 
           p.Evidence, p.StartDate AS FromDay, p.EndDate AS ToDay, p.CreatedAt, 
           p.AuthorizedAt, p.PermissionTypeId, p.Status, 
           u.Name AS UserName, u.Email, p.Id
    FROM Permission p INNER JOIN [User] u ON p.RequestingUserId = u.Id
    WHERE u.AreaId = @AreaId AND p.Status = @Status;
END;
GO

-- =============================================
-- 3. InsertPermission
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[InsertPermission]  
    @Reason VARCHAR(255), @StartDate DATETIME, @EndDate DATETIME, @CutoffId INT, 
    @PermissionTypeId INT, @RequestingUserId INT, @CreatedByUserId INT, 
    @UpdatedByUserId INT, @Evidence VARCHAR(255) = NULL,
    @Shift INT = NULL, @HoursInClassroom INT = NULL, @HoursDischarge INT = NULL,
    @PermissionId INT OUTPUT  
AS  
BEGIN  
    SET NOCOUNT ON;  
    DECLARE @Days INT = CASE WHEN @PermissionTypeId IN (2, 4) THEN DATEDIFF(DAY, @StartDate, @EndDate) + 1 ELSE 1 END;  
    DECLARE @InsertedIds TABLE (Id INT);  
    INSERT INTO [dbo].[Permission] (Reason, StartDate, EndDate, Days, RequestingUserId, CreatedByUserId, UpdatedByUserId, PermissionTypeId, CutoffId, WithPay, Status, PermissionStatusTypeId, Evidence, Shift, HoursInClassroom, HoursDischarge, CreatedAt, UpdatedAt)
    OUTPUT INSERTED.Id INTO @InsertedIds(Id) 
    VALUES (@Reason, @StartDate, @EndDate, @Days, @RequestingUserId, @CreatedByUserId, @UpdatedByUserId, @PermissionTypeId, @CutoffId, 0, 1, 1, @Evidence, @Shift, @HoursInClassroom, @HoursDischarge, GETDATE(), GETDATE());  
    SET @PermissionId = (SELECT Id FROM @InsertedIds);  
END;
GO

-- =============================================
-- 4. UpdatePermission
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[UpdatePermission]
    @PermissionId INT, @Reason VARCHAR(255) = NULL, @StartDate DATE = NULL, @EndDate DATE = NULL,
    @CutoffId INT = NULL, @PermissionTypeId INT = NULL, @UpdatedByUserId INT,
    @Status BIT = NULL, @PermissionStatusTypeId INT = NULL, @Now DATETIME
AS
BEGIN
    UPDATE [dbo].[Permission] SET
        Reason = ISNULL(@Reason, Reason), StartDate = ISNULL(@StartDate, StartDate), EndDate = ISNULL(@EndDate, EndDate),
        CutoffId = ISNULL(@CutoffId, CutoffId), PermissionTypeId = ISNULL(@PermissionTypeId, PermissionTypeId),
        UpdatedByUserId = @UpdatedByUserId, UpdatedAt = @Now, 
        Status = ISNULL(@Status, Status),
        PermissionStatusTypeId = ISNULL(@PermissionStatusTypeId, PermissionStatusTypeId)
    WHERE Id = @PermissionId;
END;
GO

-- =============================================
-- 5. UpdateStatusOfPermission
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[UpdateStatusOfPermission]
    @PermissionId INT, @WithPay BIT, @UpdatedByUserId INT, @AuthorizingUserId INT, @PermissionStatusTypeId INT
AS
BEGIN
    UPDATE [dbo].[Permission] SET
        WithPay = @WithPay, UpdatedByUserId = @UpdatedByUserId, AuthorizingUserId = @AuthorizingUserId,
        PermissionStatusTypeId = @PermissionStatusTypeId, UpdatedAt = GETDATE(), AuthorizedAt = GETDATE()
    WHERE Id = @PermissionId;
END;
GO

-- =============================================
-- 6. UpdateModifiedAt (Optimized with Session)
-- =============================================
CREATE OR ALTER PROCEDURE UpdateModifiedAt
    @TableId INT, @Ids IdTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TableName NVARCHAR(128);
    SELECT @TableName = Description FROM DatabaseTable WHERE Id = @TableId;
    
    -- Retrieve the user from the session set by C#
    DECLARE @UserId INT = TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT);
    
    DECLARE @SQL NVARCHAR(MAX) = N'UPDATE ' + QUOTENAME(@TableName) + 
                                 N' SET UpdatedAt = GETDATE()';
                                 
    -- If there is a user in session, also update UpdatedByUserId
    IF @UserId IS NOT NULL
        SET @SQL += N', UpdatedByUserId = ' + CAST(@UserId AS NVARCHAR(20));
        
    SET @SQL += N' WHERE Id IN (SELECT Id FROM @Ids)';
    
    EXEC sp_executesql @SQL, N'@Ids IdTableType READONLY', @Ids;
END;
GO

-- =============================================
-- 7. GLOBAL TRIGGER: Audit and Traceability (Permission)
-- =============================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Permission_Audit') DROP TRIGGER trg_Permission_Audit;
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Permission_UpdateModifiedAt') DROP TRIGGER trg_Permission_UpdateModifiedAt;
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Permission_Global') DROP TRIGGER trg_Permission_Global;
GO

CREATE TRIGGER trg_Permission_Global ON Permission AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Capture the application context
    DECLARE @UserId INT = COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1);
    DECLARE @OriginTypeId INT = COALESCE(TRY_CAST(SESSION_CONTEXT(N'OriginTypeId') AS INT), 3); -- 3 = Automatic System
    DECLARE @ApiFunctionId INT = COALESCE(TRY_CAST(SESSION_CONTEXT(N'ApiFunctionId') AS INT), 
        CASE 
            WHEN EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted) THEN 3 -- UPDATE
            WHEN EXISTS(SELECT 1 FROM inserted) THEN 2 -- INSERT
            ELSE 4 -- DELETE
        END);
    
    DECLARE @TableId INT = (SELECT Id FROM [DatabaseTable] WHERE Description = 'Permission');

    -- 2. Run the audit
    IF EXISTS(SELECT 1 FROM inserted) -- INSERT or UPDATE
    BEGIN
        INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
        SELECT @TableId, i.Id, @ApiFunctionId, @OriginTypeId, (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @UserId, GETDATE(), 1
        FROM inserted i LEFT JOIN deleted d ON i.Id = d.Id;
        
        -- 3. If it is an UPDATE, trigger the modification date/user update automatically
        -- We avoid infinite loops by validating that there was a real change (we do not audit the audit)
        IF EXISTS(SELECT 1 FROM deleted) AND NOT UPDATE(UpdatedAt)
        BEGIN
            DECLARE @Ids IdTableType;
            INSERT INTO @Ids SELECT Id FROM inserted;
            EXEC UpdateModifiedAt @TableId, @Ids;
        END
    END
    ELSE -- DELETE
    BEGIN
        INSERT INTO AuditLog (TableId, RecordId, ApiFunctionId, OriginTypeId, PreviousData, CreatedByUserId, CreatedAt, Status)
        SELECT @TableId, d.Id, @ApiFunctionId, @OriginTypeId, (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @UserId, GETDATE(), 1
        FROM deleted d;
    END
END;
GO

-- =============================================
-- 8. CATALOG TRIGGERS (Simple Traceability)
-- =============================================
CREATE OR ALTER TRIGGER trg_CutoffStatusType_UpdateModifiedAt ON CutoffStatusType AFTER UPDATE AS 
BEGIN 
    IF NOT UPDATE(UpdatedAt)
    BEGIN
        DECLARE @Ids IdTableType; 
        DECLARE @TableId INT = (SELECT Id FROM DatabaseTable WHERE Description = 'CutoffStatusType');
        INSERT INTO @Ids SELECT Id FROM inserted; 
        EXEC UpdateModifiedAt @TableId, @Ids; 
    END
END;
GO

CREATE OR ALTER TRIGGER trg_PermissionStatusType_UpdateModifiedAt ON PermissionStatusType AFTER UPDATE AS 
BEGIN 
    IF NOT UPDATE(UpdatedAt)
    BEGIN
        DECLARE @Ids IdTableType; 
        DECLARE @TableId INT = (SELECT Id FROM DatabaseTable WHERE Description = 'PermissionStatusType');
        INSERT INTO @Ids SELECT Id FROM inserted; 
        EXEC UpdateModifiedAt @TableId, @Ids; 
    END
END;
GO

-- =============================================
-- 9. TRIGGER: Close Cascade (Cutoff -> Permission)
-- =============================================
-- Safety net: mirrors C# CloseCutoffAsync / AutoCloseCutoffIfNeededAsync.
-- Pending permissions → Rejected (4) without pay when corte closes/invalidates.
CREATE OR ALTER TRIGGER trg_Cutoff_CloseCascade ON Cutoff AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    
    IF UPDATE(CutoffStatusTypeId)
    BEGIN
        UPDATE p
        SET p.PermissionStatusTypeId = 4, -- Rejected (matches C# behavior)
            p.WithPay = 0,
            p.UpdatedAt = GETDATE(),
            p.UpdatedByUserId = COALESCE(TRY_CAST(SESSION_CONTEXT(N'UserId') AS INT), 1)
        FROM Permission p
        INNER JOIN inserted i ON p.CutoffId = i.Id
        WHERE i.CutoffStatusTypeId IN (2, 3) -- Closed or Invalidated
          AND p.PermissionStatusTypeId = 1    -- Only Pending
          AND p.Status = 1;                    -- Only active records
    END
END;
GO
