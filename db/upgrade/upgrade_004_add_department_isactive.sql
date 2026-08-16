-- Adds IsActive to Department to track structural existence (restructuring vs. deactivation).
-- Distinct from Status (visibility/selectable in UI).
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'Department' AND c.name = 'IsActive'
)
BEGIN
    ALTER TABLE [dbo].[Department]
    ADD [IsActive] BIT NOT NULL CONSTRAINT [DF_Department_IsActive] DEFAULT(1);
END
