-- Drops IsActive from Department: the single Status flag now covers both
-- logical deletion and structural replacement (inactive records stay listed
-- and can be reactivated). Supersedes upgrade_004.
IF EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'Department' AND c.name = 'IsActive'
)
BEGIN
    ALTER TABLE [dbo].[Department]
    DROP CONSTRAINT IF EXISTS [DF_Department_IsActive];

    ALTER TABLE [dbo].[Department]
    DROP COLUMN [IsActive];
END
