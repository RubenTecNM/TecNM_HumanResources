-- upgrade_002: Columna AuthorizationComment en Permission (comentario del autorizador).
-- Idempotente: solo agrega la columna si no existe.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Permission')
      AND name = N'AuthorizationComment'
)
BEGIN
    ALTER TABLE [dbo].[Permission]
        ADD [AuthorizationComment] VARCHAR(500) NULL;
END
