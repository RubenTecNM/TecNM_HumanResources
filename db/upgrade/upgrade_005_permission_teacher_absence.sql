-- Adds Teacher Absence fields to Permission table.
-- Shift: 1=Morning, 2=Night (nullable, only used by PermissionTypeId=5)
-- HoursInClassroom: hours the teacher was present teaching (nullable)
-- HoursDischarge: non-teaching hours (tutoring, admin, research) (nullable)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'Permission' AND c.name = 'Shift'
)
BEGIN
    ALTER TABLE [dbo].[Permission]
    ADD [Shift] INT NULL,
        [HoursInClassroom] INT NULL,
        [HoursDischarge] INT NULL;
END
