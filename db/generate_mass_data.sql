-- Script de Generación Masiva de Datos para TecRHPermisos
-- Genera aprox 100,000 registros de permisos distribuidos en periodos (Cortes) desde 2020.
-- Garantiza no traslapes de periodos y fechas dentro de rangos (00:00:00 - 23:59:59).

USE RecursosHumanosDB;
GO

SET NOCOUNT ON;

DECLARE @StartDate DATETIME = '2020-01-01 00:00:00';
DECLARE @Today DATETIME = GETDATE();
DECLARE @CurrentInicia DATETIME = @StartDate;
DECLARE @CurrentTermina DATETIME;
DECLARE @CorteId INT;
DECLARE @TotalPermissions INT = 0;
DECLARE @TargetTotal INT = 100000;
DECLARE @PermissionsPerCorte INT = 650; -- Quincenas aproximadas desde 2020 (~150 cortes)

PRINT 'Iniciando generación masiva de datos (Meta: 100,000 registros)...';

WHILE @CurrentInicia < @Today AND @TotalPermissions < @TargetTotal
BEGIN
    SET @CurrentTermina = DATEADD(SECOND, -1, DATEADD(DAY, 15, @CurrentInicia)); -- Quincena exacta (termina 23:59:59)
    
    -- Insertar Corte (Estado 2 = Cerrado para historial)
    INSERT INTO Corte (Inicia, Termina, IdEstadoCorte, FechaCreacion, IdUsuarioCrea, IdUsuarioModifica)
    VALUES (@CurrentInicia, @CurrentTermina, 2, @CurrentInicia, 1, 1);
    
    SET @CorteId = SCOPE_IDENTITY();
    
    -- Insertar Permisos para este Corte
    DECLARE @j INT = 1;
    WHILE @j <= @PermissionsPerCorte AND @TotalPermissions < @TargetTotal
    BEGIN
        DECLARE @UserId INT;
        -- Seleccionar usuario trabajador (IdTipoUsuario = 4)
        SELECT TOP 1 @UserId = Id FROM Usuario WHERE IdTipoUsuario = 4 ORDER BY NEWID();
        
        DECLARE @TipoPermisoId INT;
        SELECT TOP 1 @TipoPermisoId = Id FROM TipoPermiso ORDER BY NEWID();
        
        DECLARE @EstadoId INT;
        SET @EstadoId = (ABS(CHECKSUM(NEWID())) % 4) + 1; -- IDs 1 a 4
        
        DECLARE @FechaPermiso DATETIME;
        -- Fecha aleatoria dentro del rango del corte
        SET @FechaPermiso = DATEADD(SECOND, ABS(CHECKSUM(NEWID())) % DATEDIFF(SECOND, @CurrentInicia, @CurrentTermina), @CurrentInicia);

        INSERT INTO Permiso (
            Motivo, Fecha1, Fecha2, IdTipoPermiso, IdUsuarioSolicita, 
            IdCorte, IdEstadoPermiso, Goce, FechaCreacion, IdUsuarioCrea, 
            IdUsuarioModifica, FechaModificacion
        )
        VALUES (
            'Simulación de carga masiva #' + CAST(@TotalPermissions AS VARCHAR), 
            @FechaPermiso, @FechaPermiso, @TipoPermisoId, @UserId, 
            @CorteId, @EstadoId, CASE WHEN @EstadoId = 2 THEN 1 ELSE 0 END, 
            @FechaPermiso, 1, 1, @FechaPermiso
        );
        
        SET @j = @j + 1;
        SET @TotalPermissions = @TotalPermissions + 1;
    END

    SET @CurrentInicia = DATEADD(SECOND, 1, @CurrentTermina); -- Siguiente segundo para evitar traslape
    
    IF @TotalPermissions % 10000 = 0
        PRINT 'Progreso: ' + CAST(@TotalPermissions AS VARCHAR) + ' permisos insertados...';
END

PRINT '*********************************************************';
PRINT 'GENERACIÓN COMPLETADA EXITOSAMENTE';
    DECLARE @TotalCortes INT = (SELECT COUNT(*) FROM Corte WHERE FechaCreacion >= '2020-01-01');
    PRINT 'Total de permisos: ' + CAST(@TotalPermissions AS VARCHAR);
    PRINT 'Total de cortes: ' + CAST(@TotalCortes AS VARCHAR);
PRINT '*********************************************************';
GO
