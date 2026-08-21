-- Creates ShiftType table and migrates Permission.Shift from INT to FK.
-- ShiftType: 1=Morning, 2=Afternoon, 3=Mixed
-- Matutino: 7:00-14:00 (7h), Vespertino: 16:00-22:00 (6h), Gap: 14:00-16:00

-- 1. Create ShiftType table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ShiftType')
BEGIN
    CREATE TABLE [dbo].[ShiftType] (
        [Id]               INT           IDENTITY(1,1) NOT NULL,
        [Description]      VARCHAR(50)   NOT NULL,
        [Status]           BIT           NOT NULL CONSTRAINT [DF_ShiftType_Status] DEFAULT(1),
        [CreatedAt]        DATETIME      NOT NULL CONSTRAINT [DF_ShiftType_CreatedAt] DEFAULT(GETDATE()),
        [CreatedByUserId]  INT           NOT NULL,
        [UpdatedAt]        DATETIME      NULL,
        [UpdatedByUserId]  INT           NULL,
        CONSTRAINT [PK_ShiftType] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ShiftType_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[User]([Id]),
        CONSTRAINT [FK_ShiftType_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [dbo].[User]([Id])
    );

    -- Seed: 1=Morning, 2=Afternoon, 3=Mixed
    SET IDENTITY_INSERT [dbo].[ShiftType] ON;
    INSERT INTO [dbo].[ShiftType] ([Id], [Description], [Status], [CreatedAt], [CreatedByUserId])
    VALUES (1, 'Morning',   1, GETDATE(), 1),
           (2, 'Afternoon', 1, GETDATE(), 1),
           (3, 'Mixed',     1, GETDATE(), 1);
    SET IDENTITY_INSERT [dbo].[ShiftType] OFF;

    -- UpdateModifiedAt trigger (same pattern as CutoffStatusType)
    DECLARE @sql NVARCHAR(MAX) = N'
    CREATE OR ALTER TRIGGER [dbo].[trg_ShiftType_UpdateModifiedAt] ON [dbo].[ShiftType] AFTER UPDATE AS
    BEGIN
        IF NOT UPDATE(UpdatedAt)
        BEGIN
            DECLARE @Ids IdTableType;
            DECLARE @TableId INT = (SELECT Id FROM DatabaseTable WHERE Description = ''ShiftType'');
            INSERT INTO @Ids SELECT Id FROM inserted;
            EXEC UpdateModifiedAt @TableId, @Ids;
        END
    END';
    EXEC sp_executesql @sql;
END;
GO

-- 2. Migrate Permission.Shift from plain INT to FK → ShiftType
-- First drop any existing FK on Shift (if upgrade_005 created the column as plain INT)
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys fk
    JOIN sys.columns c ON fk.parent_object_id = c.object_id AND fk.parent_object_id = OBJECT_ID('Permission')
    WHERE c.name = 'Shift'
)
BEGIN
    ALTER TABLE [dbo].[Permission] DROP CONSTRAINT [FK_Permission_Shift];
END;
GO

-- Add FK constraint
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys fk
    JOIN sys.columns c ON fk.parent_object_id = c.object_id AND fk.referenced_object_id = OBJECT_ID('ShiftType')
    WHERE fk.parent_object_id = OBJECT_ID('Permission') AND c.name = 'Shift'
)
BEGIN
    ALTER TABLE [dbo].[Permission]
    ADD CONSTRAINT [FK_Permission_Shift] FOREIGN KEY ([Shift]) REFERENCES [dbo].[ShiftType]([Id]);
END;
GO
