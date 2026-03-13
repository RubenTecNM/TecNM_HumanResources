-- Seed data for RecursosHumanos
-- Minimal data to make the system functional on first boot.
-- Passwords are hashed using ASP.NET Core Identity (PBKDF2).
-- Default password for all users: Admin1234!

USE RecursosHumanos;
GO

-- =============================================
-- CATALOGUES
-- =============================================
INSERT INTO [TipoOrigen] ([Descripcion], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
('Web', GETDATE(), 1, 1),
('API', GETDATE(), 1, 1);
GO

INSERT INTO [TipoUsuario] ([Descripcion], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
('Administrador', GETDATE(), 1, 1),
('Autorizador', GETDATE(), 1, 1),
('Recursos Humanos', GETDATE(), 1, 1),
('Empleado', GETDATE(), 1, 1);
GO

INSERT INTO [TipoPermiso] ([Descripcion], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
('Retardo', GETDATE(), 1, 1),
('Turno', GETDATE(), 1, 1),
('Falta', GETDATE(), 1, 1),
('Horario', GETDATE(), 1, 1);
GO

INSERT INTO [Area] ([Descripcion], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
('DIRECCION GENERAL', GETDATE(), 1, 1),
('SUBDIRECCION DE SERVICIOS ADMINISTRATIVOS', GETDATE(), 1, 1),
('DIRECCION ACADEMICA', GETDATE(), 1, 1),
('DIRECCION DE PLANEACION Y VINCULACION', GETDATE(), 1, 1);
GO

INSERT INTO [Departamento] ([Descripcion], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
('General', GETDATE(), 1, 1);
GO

INSERT INTO [Puesto] ([Descripcion], [ClaveTabular], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
('Administrador del Sistema', 'ADM-000', GETDATE(), 1, 1);
GO

-- =============================================
-- SEED USERS
-- Default password for all: Admin1234!
-- =============================================
INSERT INTO [Usuario] ([Nombre], [Correo], [Clave], [FechaCreacion], [Estatus], [IdTipoUsuario], [IdArea], [IdDepartamento], [IdPuesto], [IdUsuarioCrea]) VALUES
('Administrador', 'controlpermisos@monclova.tecnm.mx', 'AQAAAAIAAYagAAAAEB5dU3Z+7Zy9U6gZx/P0VN3zLJEiMIkNXFO1UeYKcBV1vUbpMJ8QvR3TknL5sT46iw==', GETDATE(), 1, 1, 2, 1, 1, 1),
('Francisco Rodriguez Hernandez', 'francisco.rh@monclova.tecnm.mx', 'AQAAAAIAAYagAAAAEB5dU3Z+7Zy9U6gZx/P0VN3zLJEiMIkNXFO1UeYKcBV1vUbpMJ8QvR3TknL5sT46iw==', GETDATE(), 1, 1, 1, 1, 1, 1),
('Alfredo Salinas Mendoza', 'alfredo.salinas@monclova.tecnm.mx', 'AQAAAAIAAYagAAAAEB5dU3Z+7Zy9U6gZx/P0VN3zLJEiMIkNXFO1UeYKcBV1vUbpMJ8QvR3TknL5sT46iw==', GETDATE(), 1, 2, 2, 1, 1, 1),
('Jesus Roberto De La Garza', 'jesus.delagarza@monclova.tecnm.mx', 'AQAAAAIAAYagAAAAEB5dU3Z+7Zy9U6gZx/P0VN3zLJEiMIkNXFO1UeYKcBV1vUbpMJ8QvR3TknL5sT46iw==', GETDATE(), 1, 2, 3, 1, 1, 1),
('Rafael Linan Torres', 'rafael.linan@monclova.tecnm.mx', 'AQAAAAIAAYagAAAAEB5dU3Z+7Zy9U6gZx/P0VN3zLJEiMIkNXFO1UeYKcBV1vUbpMJ8QvR3TknL5sT46iw==', GETDATE(), 1, 2, 4, 1, 1, 1),
('Izamar Guadalupe Mendez Hernandez', 'izamar.mh@monclova.tecnm.mx', 'AQAAAAIAAYagAAAAEB5dU3Z+7Zy9U6gZx/P0VN3zLJEiMIkNXFO1UeYKcBV1vUbpMJ8QvR3TknL5sT46iw==', GETDATE(), 1, 3, 2, 1, 1, 1),
('Empleado Muestra', 'empleado@monclova.tecnm.mx', 'AQAAAAIAAYagAAAAEB5dU3Z+7Zy9U6gZx/P0VN3zLJEiMIkNXFO1UeYKcBV1vUbpMJ8QvR3TknL5sT46iw==', GETDATE(), 1, 4, 2, 1, 1, 1);
GO

-- =============================================
-- ACTIVE PAYROLL PERIOD
-- =============================================
INSERT INTO [Corte] ([Inicia], [Termina], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
(DATEADD(day, -7, GETDATE()), DATEADD(day, 7, GETDATE()), GETDATE(), 1, 1);
GO

-- =============================================
-- AUDIT TABLES CATALOGUES
-- =============================================
INSERT INTO [Tabla] ([Descripcion], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
('Usuario', GETDATE(), 1, 1),
('Permiso', GETDATE(), 1, 1),
('Corte', GETDATE(), 1, 1);
GO

INSERT INTO [ApiFunctions] ([Descripcion], [FechaCreacion], [Estatus], [IdUsuarioCrea]) VALUES
('CREATE', GETDATE(), 1, 1),
('UPDATE', GETDATE(), 1, 1),
('DELETE', GETDATE(), 1, 1),
('READ', GETDATE(), 1, 1);
GO
