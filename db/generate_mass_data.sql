-- Mass Data Generation Script for TecRHPermisos
-- Generates ~100,000 permission records distributed across periods (Cutoffs) since 2020.
-- Guarantees no period overlaps and dates within ranges (00:00:00 - 23:59:59).

SET NOCOUNT ON;

DECLARE @StartDate DATETIME = '2020-01-01 00:00:00';
DECLARE @Today DATETIME = GETDATE();
DECLARE @CurrentStartDate DATETIME = @StartDate;
DECLARE @CurrentEndDate DATETIME;
DECLARE @CutoffId INT;
DECLARE @TotalPermissions INT = 0;
DECLARE @TargetTotal INT = 100000;
DECLARE @PermissionsPerCutoff INT = 650; -- Approximate biweekly periods since 2020 (~150 cutoffs)

PRINT 'Starting mass data generation (Target: 100,000 records)...';

WHILE @CurrentStartDate < @Today AND @TotalPermissions < @TargetTotal
BEGIN
    SET @CurrentEndDate = DATEADD(SECOND, -1, DATEADD(DAY, 15, @CurrentStartDate)); -- Exact biweek (ends 23:59:59)
    
    -- Determine status: 2 (Closed) if it already ended, 1 (Open) if it is the current period
    DECLARE @CutoffStatusTypeId INT = CASE WHEN @CurrentEndDate < GETDATE() THEN 2 ELSE 1 END;

    INSERT INTO Cutoff (StartDate, EndDate, CutoffStatusTypeId, CreatedAt, CreatedByUserId, UpdatedByUserId, Status)
    VALUES (@CurrentStartDate, @CurrentEndDate, @CutoffStatusTypeId, @CurrentStartDate, 1, 1, 1);
    
    SET @CutoffId = @@IDENTITY;
    
    -- Insert permissions for this cutoff
    DECLARE @j INT = 1;
    WHILE @j <= @PermissionsPerCutoff AND @TotalPermissions < @TargetTotal
    BEGIN
        DECLARE @UserId INT;
        -- Select a worker user (UserTypeId = 4)
        SELECT TOP 1 @UserId = Id FROM [User] WHERE UserTypeId = 4 ORDER BY NEWID();
        
        DECLARE @PermissionTypeId INT;
        SELECT TOP 1 @PermissionTypeId = Id FROM PermissionType ORDER BY NEWID();
        
        DECLARE @PermissionStatusTypeId INT;
        SET @PermissionStatusTypeId = (ABS(CHECKSUM(NEWID())) % 4) + 1; -- IDs 1 to 4
        
        DECLARE @PermissionDate DATETIME;
        -- Random date within the cutoff range
        SET @PermissionDate = DATEADD(SECOND, ABS(CHECKSUM(NEWID())) % DATEDIFF(SECOND, @CurrentStartDate, @CurrentEndDate), @CurrentStartDate);

        INSERT INTO Permission (
            Reason, StartDate, EndDate, PermissionTypeId, RequestingUserId, 
            CutoffId, PermissionStatusTypeId, WithPay, CreatedAt, CreatedByUserId, 
            UpdatedByUserId, UpdatedAt, Status
        )
        VALUES (
            'Simulación de carga masiva #' + CAST(@TotalPermissions AS VARCHAR), 
            @PermissionDate, @PermissionDate, @PermissionTypeId, @UserId, 
            @CutoffId, @PermissionStatusTypeId, CASE WHEN @PermissionStatusTypeId = 2 THEN 1 ELSE 0 END, 
            @PermissionDate, 1, 1, @PermissionDate, 1
        );
        
        SET @j = @j + 1;
        SET @TotalPermissions = @TotalPermissions + 1;
    END

    SET @CurrentStartDate = DATEADD(SECOND, 1, @CurrentEndDate); -- Next second to avoid overlap
    
    IF @TotalPermissions % 10000 = 0
        PRINT 'Progress: ' + CAST(@TotalPermissions AS VARCHAR) + ' permissions inserted...';
END

PRINT '*********************************************************';
PRINT 'GENERATION COMPLETED SUCCESSFULLY';
    DECLARE @TotalCutoffs INT = (SELECT COUNT(*) FROM Cutoff WHERE CreatedAt >= '2020-01-01');
    PRINT 'Total permissions: ' + CAST(@TotalPermissions AS VARCHAR);
    PRINT 'Total cutoffs: ' + CAST(@TotalCutoffs AS VARCHAR);
PRINT '*********************************************************';
PRINT '*********************************************************';
