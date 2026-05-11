-- =============================================
-- DESHABILITAR FK PARA INSERCIÓN MASIVA
-- =============================================
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- =============================================
-- DATOS INICIALES
-- =============================================

-- TipoUsuario
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Administrador', GETDATE(), 1, 1),
('Autorizador', GETDATE(), 1, 1),
('Recursos Humanos', GETDATE(), 1, 1),
('Trabajador', GETDATE(), 1, 1);

-- Areas
INSERT INTO Area (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('SUBDIRECCION DE SERVICIOS ADMINISTRATIVOS', GETDATE(), 1, 1),
('DIRECCION GENERAL', GETDATE(), 1, 1),
('DIRECCION DE PLANEACION Y VINCULACION', GETDATE(), 1, 1),
('DIRECCION ACADEMICA', GETDATE(), 1, 1);

-- Departamentos (Ejemplos)
INSERT INTO Departamento (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('CENTRO DE CÓMPUTO', GETDATE(), 1, 1),
('RECURSOS HUMANOS', GETDATE(), 1, 1),
('DIRECCION GENERAL', GETDATE(), 1, 1);

-- Puestos (Ejemplos)
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('INGENIERO EN SISTEMAS', 'CF12027', GETDATE(), 1, 1),
('JEFE(A) DE DEPARTAMENTO', 'ITS0004', GETDATE(), 1, 1);

-- Tipos de Origen
INSERT INTO TipoOrigen (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Interfaz Web', GETDATE(), 1, 1),
('API Externa', GETDATE(), 1, 1);

-- Tipos de Permiso
INSERT INTO TipoPermiso (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Retardo', GETDATE(), 1, 1),
('Turno por Turno', GETDATE(), 1, 1),
('Falta', GETDATE(), 1, 1),
('Cambio de Horario', GETDATE(), 1, 1);

-- Usuario Root (Clave: Buss1ness!2026 hash -> 4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7)
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea) 
VALUES ('root', 'controlpermisos@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', GETDATE(), 1, 1, 1, 1, 1, 1);

-- =============================================
-- RE-HABILITAR FK
-- =============================================
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO

-- =============================================
-- PROCEDIMIENTOS ALMACENADOS (Básico)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[LoginUsuario]
    @mail NVARCHAR(100), @password NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.Id, u.Nombre, u.Correo, u.IdArea, a.Descripcion AS NombreArea, u.IdTipoUsuario
    FROM Usuario u INNER JOIN Area a ON u.IdArea = a.Id
    WHERE u.Correo = @mail AND u.Clave = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(100), @password)), 2);
END;
GO
