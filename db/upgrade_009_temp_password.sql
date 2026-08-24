-- upgrade_009: clave temporal visible para RH hasta que el usuario ingrese
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.[User]') AND name = 'TempPassword')
BEGIN
    ALTER TABLE dbo.[User] ADD TempPassword NVARCHAR(64) NULL;
END
GO
