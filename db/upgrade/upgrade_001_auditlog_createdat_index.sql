-- upgrade_001: Índice de auditoría por fecha de creación.
-- Idempotente: solo crea el índice si no existe.
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_AuditLog_CreatedAt'
      AND object_id = OBJECT_ID(N'dbo.AuditLog')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AuditLog_CreatedAt]
        ON [dbo].[AuditLog] ([CreatedAt] ASC);
END
