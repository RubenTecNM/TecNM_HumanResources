-- Adds AreaId to Department to link each department with its organizational area.
-- Nullable: legacy rows may not have an assigned area yet.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'Department' AND c.name = 'AreaId'
)
BEGIN
    ALTER TABLE [dbo].[Department]
    ADD [AreaId] INT NULL;
END
