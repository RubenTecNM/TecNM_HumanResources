CREATE OR ALTER TRIGGER trg_Permiso_Global ON Permiso AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Capturar contexto de la aplicación
    DECLARE @IdUsuario INT = COALESCE(TRY_CAST(SESSION_CONTEXT(N'IdUsuario') AS INT), 1);
    DECLARE @IdTipoOrigen INT = COALESCE(TRY_CAST(SESSION_CONTEXT(N'IdTipoOrigen') AS INT), 3); -- 3 = Sistema Automático
    DECLARE @IdApiFunction INT = COALESCE(TRY_CAST(SESSION_CONTEXT(N'IdApiFunction') AS INT), 
        CASE 
            WHEN EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted) THEN 3 -- UPDATE
            WHEN EXISTS(SELECT 1 FROM inserted) THEN 2 -- INSERT
            ELSE 4 -- DELETE
        END);
    
    DECLARE @IdTabla INT = (SELECT Id FROM [Tabla] WHERE Descripcion = 'Permiso');

    -- 2. Ejecutar Auditoría
    IF EXISTS(SELECT 1 FROM inserted) -- INSERT o UPDATE
    BEGIN
        INSERT INTO Auditoria (IdTabla, IdRegistro, IdApiFunction, IdTipoOrigen, DatosAnteriores, IdUsuarioCrea, FechaCreacion, Estatus)
        SELECT @IdTabla, i.Id, @IdApiFunction, @IdTipoOrigen, (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @IdUsuario, GETDATE(), 1
        FROM inserted i LEFT JOIN deleted d ON i.Id = d.Id;
        
        -- 3. Si es UPDATE, disparar actualización de fecha y usuario modifica automaticamente
        IF EXISTS(SELECT 1 FROM deleted) AND NOT UPDATE(FechaModificacion)
        BEGIN
            DECLARE @Ids IdTableType;
            INSERT INTO @Ids SELECT Id FROM inserted;
            EXEC UpdateFechaModificacion @IdTabla, @Ids;
        END
    END
    ELSE -- DELETE
    BEGIN
        INSERT INTO Auditoria (IdTabla, IdRegistro, IdApiFunction, IdTipoOrigen, DatosAnteriores, IdUsuarioCrea, FechaCreacion, Estatus)
        SELECT @IdTabla, d.Id, @IdApiFunction, @IdTipoOrigen, (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), @IdUsuario, GETDATE(), 1
        FROM deleted d;
    END
END;
GO
