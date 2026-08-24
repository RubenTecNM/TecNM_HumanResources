-- Drops IsActive from Department: the single Status flag now covers both
-- logical deletion and structural replacement (inactive records stay listed
-- and can be reactivated). Supersedes upgrade_004.
-- The column's DEFAULT constraint may have a system-generated name (created
-- by the EF model, not by upgrade_004), so it is looked up dynamically.
DECLARE @df sysname = (
    SELECT dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID('[dbo].[Department]') AND c.name = 'IsActive'
);
IF @df IS NOT NULL
    EXEC(N'ALTER TABLE [dbo].[Department] DROP CONSTRAINT [' + @df + N'];');

IF EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'Department' AND c.name = 'IsActive'
)
BEGIN
    ALTER TABLE [dbo].[Department]
    DROP COLUMN [IsActive];
END
