-- =============================================
-- TIPOS DE TABLA
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'IdTableType' AND is_table_type = 1)
BEGIN
    CREATE TYPE IdTableType AS TABLE (Id INT);
END;
--###--

-- =============================================
-- 1. LoginUsuario
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[LoginUsuario]
    @mail NVARCHAR(100), @password NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.Id, u.Nombre, u.Correo, u.IdArea, a.Descripcion AS NombreArea, u.IdTipoUsuario
    FROM Usuario u INNER JOIN Area a ON u.IdArea = a.Id
    WHERE u.Correo = @mail AND u.Clave = @password;
END;
--###--

-- =============================================
-- 2. BuscarPermisosFiltrados
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[BuscarPermisosFiltrados]
    @FromDate DATETIME = NULL, @ToDate DATETIME = NULL, @UserId INT = NULL,
    @UserEmail NVARCHAR(100) = NULL, @AreaId INT = NULL, @PermissionTypeId INT = 0,
    @IsEmployeer BIT = 0, @IsAutorizer BIT = 0, @Page INT = 1, @Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @Limit;
    ;WITH PermisosFiltrados AS (
        SELECT p.Motivo AS Descripcion, p.Dias AS DiasSolicitados, p.Goce AS ConGoce,
            p.Fecha1 AS DelDia, p.Fecha2 AS AlDia, p.Evidencia, p.FechaCreacion,
            p.FechaAutorizacion, p.FechaCreacion AS FechaSolicitud, p.IdCorte,
            p.IdTipoPermiso AS IdTipo_Permisos, p.Estatus, u.Nombre, u.Correo, u.IdArea,
            u.Id AS IdUsuario, p.Id AS IdPermiso
        FROM Permiso p INNER JOIN Usuario u ON p.IdUsuarioSolicita = u.Id
        WHERE (@FromDate IS NULL OR p.FechaCreacion >= @FromDate)
            AND (@ToDate IS NULL OR p.FechaCreacion <= @ToDate)
            AND ((@IsEmployeer = 1 AND @IsAutorizer = 0 AND u.Id = @UserId)
                OR (@UserEmail IS NOT NULL AND u.Correo = @UserEmail)
                OR (@IsEmployeer = 0 AND @IsAutorizer = 1 AND u.IdArea = @AreaId)
                OR (@AreaId IS NOT NULL AND u.IdArea = @AreaId))
            AND (@PermissionTypeId = 0 OR p.IdTipoPermiso = @PermissionTypeId)
    )
    SELECT * FROM PermisosFiltrados ORDER BY FechaCreacion DESC
    OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;
    SELECT COUNT(*) AS Total FROM PermisosFiltrados;
END;
--###--

-- =============================================
-- 3. GetPermissionsAuthorizer
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetPermissionsAuthorizer]
    @IdArea INT, @Estatus BIT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Motivo AS Descripcion, p.Dias AS DiasSolicitados, p.Goce AS ConGoce, 
           p.Evidencia, p.Fecha1 AS DelDia, p.Fecha2 AS AlDia, p.FechaCreacion, 
           p.FechaAutorizacion, p.IdTipoPermiso AS IdTipo_Permisos, p.Estatus, 
           u.Nombre AS NombreUsuario, u.Correo, p.Id
    FROM Permiso p INNER JOIN Usuario u ON p.IdUsuarioSolicita = u.Id
    WHERE u.IdArea = @IdArea AND p.Estatus = @Estatus;
END;
--###--

-- =============================================
-- 4. InsertPermission
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[InsertPermission]  
    @Motivo VARCHAR(255), @Fecha1 DATETIME, @Fecha2 DATETIME, @IdCorte INT, 
    @IdTipoPermiso INT, @IdUsuarioSolicita INT, @IdUsuarioCrea INT, 
    @IdUsuarioModifica INT, @Evidencia VARCHAR(255) = NULL, @IdPermiso INT OUTPUT  
AS  
BEGIN  
    SET NOCOUNT ON;  
    DECLARE @Dias INT = CASE WHEN @IdTipoPermiso IN (2, 4, 5) THEN DATEDIFF(DAY, @Fecha1, @Fecha2) + 1 ELSE 1 END;  
    DECLARE @InsertedIds TABLE (Id INT);  
    INSERT INTO [dbo].[Permiso] (Motivo, Fecha1, Fecha2, Dias, IdUsuarioSolicita, IdUsuarioCrea, IdUsuarioModifica, IdTipoPermiso, IdCorte, Goce, Estatus, IdEstadoPermiso, Evidencia, FechaCreacion, FechaModificacion)
    OUTPUT INSERTED.Id INTO @InsertedIds(Id) 
    VALUES (@Motivo, @Fecha1, @Fecha2, @Dias, @IdUsuarioSolicita, @IdUsuarioCrea, @IdUsuarioModifica, @IdTipoPermiso, @IdCorte, 0, 1, 1, @Evidencia, GETDATE(), GETDATE());  
    SET @IdPermiso = (SELECT Id FROM @InsertedIds);  
END;
--###--

-- =============================================
-- 5. UpdatePermission
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[UpdatePermission]
    @IdPermiso INT, @Descripcion VARCHAR(255) = NULL, @DelDia DATE = NULL, @AlDia DATE = NULL,
    @IdCorte INT = NULL, @IdTipo_Permisos INT = NULL, @IdUsuarioModifica INT,
    @Estatus BIT = NULL, @IdEstadoPermiso INT = NULL, @Now DATETIME
AS
BEGIN
    UPDATE [dbo].[Permiso] SET
        Motivo = ISNULL(@Descripcion, Motivo), Fecha1 = ISNULL(@DelDia, Fecha1), Fecha2 = ISNULL(@AlDia, Fecha2),
        IdCorte = ISNULL(@IdCorte, IdCorte), IdTipoPermiso = ISNULL(@IdTipo_Permisos, IdTipoPermiso),
        IdUsuarioModifica = @IdUsuarioModifica, FechaModificacion = @Now, 
        Estatus = ISNULL(@Estatus, Estatus),
        IdEstadoPermiso = ISNULL(@IdEstadoPermiso, IdEstadoPermiso)
    WHERE Id = @IdPermiso;
END;
--###--

-- =============================================
-- 6. UpdateStatusOfPermission
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[UpdateStatusOfPermission]
    @IdPermiso INT, @ConGoce BIT, @IdUsuarioModifica INT, @IdUsuarioAutorizador INT, @IdEstadoPermiso INT
AS
BEGIN
    UPDATE [dbo].[Permiso] SET
        Goce = @ConGoce, IdUsuarioModifica = @IdUsuarioModifica, IdUsuarioAutoriza = @IdUsuarioAutorizador,
        IdEstadoPermiso = @IdEstadoPermiso, FechaModificacion = GETDATE(), FechaAutorizacion = GETDATE()
    WHERE Id = @IdPermiso;
END;
--###--

-- =============================================
-- 7. UpdateFechaModificacion
-- =============================================
CREATE OR ALTER PROCEDURE UpdateFechaModificacion
    @TableId INT, @Ids IdTableType READONLY
AS
BEGIN
    DECLARE @TableName NVARCHAR(128);
    SELECT @TableName = Descripcion FROM Tabla WHERE Id = @TableId;
    DECLARE @SQL NVARCHAR(MAX) = N'UPDATE ' + QUOTENAME(@TableName) + N' SET FechaModificacion = GETDATE() WHERE Id IN (SELECT Id FROM @Ids)';
    EXEC sp_executesql @SQL, N'@Ids IdTableType READONLY', @Ids;
END;
--###--

-- =============================================
-- 8. TRIGGER Auditoria
-- =============================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Permiso_UpdateFechaModificacion') DROP TRIGGER trg_Permiso_UpdateFechaModificacion;
--###--
CREATE TRIGGER trg_Permiso_UpdateFechaModificacion ON Permiso AFTER UPDATE AS 
BEGIN 
    DECLARE @Ids IdTableType; 
    INSERT INTO @Ids SELECT Id FROM inserted; 
    EXEC UpdateFechaModificacion 8, @Ids; 
END;

-- =============================================
-- 9. TRIGGERS NUEVOS ESTADOS
-- =============================================
CREATE TRIGGER trg_TipoEstadoCorte_UpdateFechaModificacion ON TipoEstadoCorte AFTER UPDATE AS 
BEGIN 
    DECLARE @Ids IdTableType; 
    INSERT INTO @Ids SELECT Id FROM inserted; 
    EXEC UpdateFechaModificacion (SELECT Id FROM Tabla WHERE Descripcion = 'TipoEstadoCorte'), @Ids; 
END;
--###--
CREATE TRIGGER trg_TipoEstadoPermiso_UpdateFechaModificacion ON TipoEstadoPermiso AFTER UPDATE AS 
BEGIN 
    DECLARE @Ids IdTableType; 
    INSERT INTO @Ids SELECT Id FROM inserted; 
    EXEC UpdateFechaModificacion (SELECT Id FROM Tabla WHERE Descripcion = 'TipoEstadoPermiso'), @Ids; 
END;
