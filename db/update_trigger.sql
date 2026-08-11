CREATE OR ALTER TRIGGER trg_Permission_Global ON Permission AFTER INSERT, UPDATE, DELETE AS
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
