-- Script de Poblado Inicial (Seguro para Llaves Foráneas)
-- Basado en el patrón de resiliencia de NexSmart

-- Deshabilitar temporalmente las restricciones para evitar conflictos circulares (IdUsuarioCrea)
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- Catalogos Base
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('Administrador', GETDATE(), 1, 1);
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('Autorizador', GETDATE(), 1, 1);
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('Recursos Humanos', GETDATE(), 1, 1);
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('Trabajador', GETDATE(), 1, 1);

INSERT INTO Area (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('DIRECCIÓN GENERAL', GETDATE(), 1, 1);
INSERT INTO Departamento (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('SISTEMAS', GETDATE(), 1, 1);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('ROOT', 'ITS0001', GETDATE(), 1, 1);
INSERT INTO TipoOrigen (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('Interfaz Web', GETDATE(), 1, 1);

INSERT INTO TipoPermiso (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('Retardo', GETDATE(), 1, 1);
INSERT INTO TipoPermiso (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('Falta', GETDATE(), 1, 1);
INSERT INTO TipoPermiso (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES ('Salida Anticipada', GETDATE(), 1, 1);

-- Usuario Root Inicial
-- Clave: Buss1ness.2026 
-- Hash SHA256: 5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea) 
VALUES ('root', 'controlpermisos@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 1, 1, 1, 1, 1);

-- Re-habilitar las restricciones de integridad
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
