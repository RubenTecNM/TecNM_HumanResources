-- Elimina el estado "Invalidado" (corte y permiso): se desactiva su registro
-- (Status = 0) en vez de borrarlo, para conservar la integridad referencial de
-- registros históricos. Introduce el nuevo estado propio "Cancelado" (id 5)
-- para la cancelación de un permiso pendiente por su solicitante.
DECLARE @Now DATETIME = GETDATE();

-- 1. Desactivar el estado de corte "Invalidado" si existe
UPDATE [dbo].[CutoffStatusType]
SET Status = 0
WHERE Description = 'Invalidado';

-- 2. Desactivar el estado de permiso "Invalidado por Periodo" si existe
UPDATE [dbo].[PermissionStatusType]
SET Status = 0
WHERE Description = 'Invalidado por Periodo';

-- 3. Insertar el estado "Cancelado por el Solicitante" con Id explícito = 5
IF NOT EXISTS (SELECT 1 FROM [dbo].[PermissionStatusType] WHERE Id = 5)
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.identity_columns
        WHERE object_id = OBJECT_ID(N'dbo.PermissionStatusType') AND name = 'Id'
    )
    BEGIN
        SET IDENTITY_INSERT [dbo].[PermissionStatusType] ON;
        INSERT INTO [dbo].[PermissionStatusType]
            (Id, Description, Color, CreatedAt, Status, CreatedByUserId)
        VALUES
            (5, N'Cancelado por el Solicitante', N'secondary', @Now, 1, 1);
        SET IDENTITY_INSERT [dbo].[PermissionStatusType] OFF;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[PermissionStatusType]
            (Id, Description, Color, CreatedAt, Status, CreatedByUserId)
        VALUES
            (5, N'Cancelado por el Solicitante', N'secondary', @Now, 1, 1);
    END
END
