-- =============================================
-- DESHABILITAR FK PRIMERO
-- =============================================
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- =============================================
-- LIMPIAR TABLAS
-- =============================================
DELETE FROM Auditoria;
DELETE FROM Permiso;
DELETE FROM ApiPermisosApiKeysTabla;
DELETE FROM ResetPasswordTokens;
DELETE FROM Corte;
DELETE FROM ApiKey;
DELETE FROM ApiFunctions;
DELETE FROM ApiPermiso;
DELETE FROM Tabla;
DELETE FROM TipoOrigen;
DELETE FROM TipoPermiso;
DELETE FROM Usuario;
DELETE FROM Puesto;
DELETE FROM Departamento;
DELETE FROM Area;
DELETE FROM TipoUsuario;

-- Resetear los contadores IDENTITY a 0
DBCC CHECKIDENT ('TipoUsuario', RESEED, 0);
DBCC CHECKIDENT ('Area', RESEED, 0);
DBCC CHECKIDENT ('Departamento', RESEED, 0);
DBCC CHECKIDENT ('Puesto', RESEED, 0);
DBCC CHECKIDENT ('Usuario', RESEED, 0);
DBCC CHECKIDENT ('ApiFunctions', RESEED, 0);
DBCC CHECKIDENT ('ApiPermiso', RESEED, 0);
DBCC CHECKIDENT ('Tabla', RESEED, 0);
DBCC CHECKIDENT ('TipoOrigen', RESEED, 0);
DBCC CHECKIDENT ('TipoPermiso', RESEED, 0);
DBCC CHECKIDENT ('ApiKey', RESEED, 0);
DBCC CHECKIDENT ('Corte', RESEED, 0);
DBCC CHECKIDENT ('Permiso', RESEED, 0);
DBCC CHECKIDENT ('Auditoria', RESEED, 0);
DBCC CHECKIDENT ('ResetPasswordTokens', RESEED, 0);
DBCC CHECKIDENT ('ApiPermisosApiKeysTabla', RESEED, 0);


-- =============================================
-- DESHABILITAR FK
-- =============================================
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('PRUEBAa', 'prueba@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-03-13 06:53:35', NULL, 1, 4, 4, 3, 3, 1, NULL, NULL);

-- =============================================
-- PASO 1: TipoUsuario
-- =============================================
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Usuario del sistema', '2026-01-18 15:06:33', NULL, 1, 1, NULL);
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Tipo de usuario autorizado para aprobar', '2026-01-18 15:06:33', NULL, 1, 1, NULL);
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Tipo de usuario encargado de recursos humanos', '2026-01-18 15:06:33', NULL, 1, 1, NULL);
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Tipo de usuario trabajador de la empresa', '2026-01-18 15:06:33', NULL, 1, 1, NULL);

-- =============================================
-- PASO 2: Area
-- =============================================
INSERT INTO Area (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('SUBDIRECCION DE SERVICIOS ADMINISTRATIVOS', '2026-01-18 04:53:14', NULL, 1, 1, NULL);
INSERT INTO Area (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('DIRECCION GENERAL', '2026-01-18 04:53:14', NULL, 1, 1, NULL);
INSERT INTO Area (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('DIRECCION DE PLANEACION Y VINCULACION', '2026-01-18 04:53:14', NULL, 1, 1, NULL);
INSERT INTO Area (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('DIRECCION ACADEMICA', '2026-01-18 04:53:14', NULL, 1, 1, NULL);

-- =============================================
-- PASO 3: Departamento
-- =============================================
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ACTIVIDADES EXTRAESCOLARES', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ACTIVO FIJO', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ASUNTOS ACADÉMICOS', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('BIBLIOTECA', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('CENTRO DE CÓMPUTO', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('CENTRO DE ENTRENAMIENTO NAUTILUS', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('CIE', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('COMPRAS', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('COMUNICACIÓN', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('CONTROL INTERNO', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('COORDINACIÓN ACADÉMICA', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('COORDINACIÓN DE INNOVACIÓN EDUCATIVA', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('DEPARTAMENTO JURÍDICO', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('DIRECCION GENERAL', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ENFERMERÍA', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('GESTIÓN TECNOLÓGICA Y VINCULACIÓN', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('IDIOMAS', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('INNOVACIÓN Y CALIDAD', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('MANTENIMIENTO', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('PLANEACIÓN Y PRESUPUESTO', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('PREFECTURA', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('PSICOLOGÍA', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('RECURSOS FINANCIEROS', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('RECURSOS HUMANOS', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('SERVICIOS ESCOLARES', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('SUBDIRECCIÓN DE SERVICIOS', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('TALLER DE LIT', '2026-01-21 08:12:48', NULL, 1, 1, NULL);
INSERT INTO Departamento (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('TRABAJO SOCIAL', '2026-01-21 08:12:48', NULL, 1, 1, NULL);

-- =============================================
-- PASO 4: Puesto
-- =============================================
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ALMACENISTA', 'A03004', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ANALISTA ESPECIALIZADO(A)', 'P01002', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ANALISTA TECNICO(A)', 'P01001', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('AUXILIAR ADMINISTRATIVO', 'A01024', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('BIBLIOTECARIO(A)', 'T05003', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('CAPTURISTA', 'T06027', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('CHOFER DE DIRECTOR', 'CF53453', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('INGENIERO EN SISTEMAS', 'CF12027', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('INTENDENTE', 'S06002', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('JEFE DE DIVISION', 'ITS0003', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('JEFE DE OFICINA', 'A01001', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('JEFE(A) DE DEPARTAMENTO', 'ITS0004', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('JEFE(A) DE DEPARTAMENTO', 'P16004', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('LABORATORISTA', 'T16005', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('MEDICO GENERAL', 'P13006', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('PROFESOR ASIGNATURA A', 'E13001', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('PROFESOR ASIGNATURA B', 'E13002', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('PROFESOR ASOCIADO A', 'E13010', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('SECRETARIA DE DIRECTOR GENERAL', 'CF53455', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('SECRETARIA DE SUBDIRECTOR', 'CF34280', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('SECRETARIO(A) DE JEFE(A) DE DEPARTAMENTO', 'CF34004', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('TAQUIMECANOGRAFA', 'A08004', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('TECNICO EN MANTENIMIENTO', 'S08011', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('TECNICO ESPECIALIZADO', 'CF33116', '2026-01-21 08:13:44', NULL, 1, 1, NULL);
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('VIGILANTE', 'S14001', '2026-01-21 08:13:44', NULL, 1, 1, NULL);

-- =============================================
-- PASO 5: Usuario
-- =============================================
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('root', 'controlpermisos@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:14', NULL, 1, 1, 1, 1, 1, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('RODRIGUEZ HERNANDEZ FRANCISCO', 'francisco.rh@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 1, 2, 19, 1, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MONTOYA ELIZONDO MAIRA DE JESUS', 'maira.me@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 2, 2, 5, 2, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('AGUILERA MANCILLA DANIELA', 'daniela.am@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 26, 3, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('RIVERA QUINTERO OLGA LUCINDA', 'olga.rq@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 23, 3, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('VILLANUEVA FLORES MAYRA', 'mayra.vf@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 2, 14, 3, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('OZUNA RODRIGUEZ RAQUEL', 'raquel.or@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 21, 4, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('VAZQUEZ QUINTERO JUAN', 'juan.vq@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 15, 4, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('GONZALEZ FLORES LAURA ISELDA', 'laura.gf@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 25, 5, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('CAMPOS RODRIGUEZ LETICIA ABIGAIL', 'leticia.cr@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 25, 6, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('LOERA SOLIS JUAN FRANCISCO', 'francisco.ls@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 9, 6, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('LOZANO FERNANDEZ JESUS FERNANDO', 'fernando.lf@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 21, 6, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MENDEZ HERNANDEZ IZAMAR GUADALUPE', 'izamar.mh@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 3, 1, 24, 6, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('PECINA VAZQUEZ ELSA MARICELA', 'elsa.pv@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 4, 6, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('RAMIREZ GOMEZ RAQUEL DODANIM', 'raquel.rg@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 23, 6, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SALOMON TADEO NICOLAS', 'nicolas.st@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 7, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('ROMO VEGA CESAR ALBERTO', 'cesar.rv@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 27, 8, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('DE LEON REYNA JOSE ALFREDO', 'jose.lr@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 9, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('DE LEON ZAMORA JUAN MANUEL', 'juan.lz@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 9, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('ESCOBEDO GUTIERREZ CLAUDIO JESUS', 'claudio.eg@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 9, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('GUTIERREZ REYES LUZ ELENA', 'luz.gr@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 9, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('REYNA CASTILLO SAN JUANA', 'sanjuana.rc@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 9, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('BORJAS LEIJA MARIA TERESA', 'teresa.bl@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 12, 10, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('DE LA CRUZ MATA MARTINA', 'martina.cm@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 17, 10, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MENDOZA RIOJAS ROCIO DEL CARMEN', 'rocio.mendoza@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 16, 10, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SALOMON TADEO MARIA ANASTACIO', 'maria.st@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 21, 10, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SANDOVAL GUERRERO ROSALVA', 'rosalva.sg@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 3, 10, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('VAZQUEZ SALINAS CLAUDIA', 'claudia.vs@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 25, 10, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('DE LEON MARTINEZ ANA CRISTINA', 'ana.lm@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 26, 11, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('GONZALEZ PALOALTO MARGARITA', 'margarita.gp@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 11, 11, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('HERNANDEZ DE LA GARZA VERONICA PATRICIA', 'veronica.hg@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 25, 11, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('CASTRO MARTINEZ GRACIELA', 'graciela.cm@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 18, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('CHAIREZ HUERTA SERGIO IVAN', 'ivan.chairez@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 2, 2, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('DE LA ROSA MARES JOEL', 'joel.rm@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 1, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('GALARZA RINCON TELMA CECILIA', 'telma.gr@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 10, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('GUARDIOLA NUÑEZ JAVIER JULIAN', 'javier.gn@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MARTINEZ DE HOYOS ARTURO', 'arturo.martinez@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 16, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MENDOZA VILLA FIDENCIO ALBERTO', 'fidencio.mendoza@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 2, 5, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MUÑOZ DOMINGUEZ ROSARIO IRENE', 'rosario.md@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 23, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('RAMOS GARCIA JUANITA GUADALUPE', 'juanita.rg@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 24, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('ROMO HERRERA JESUS', 'jesus.rh@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 6, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SANDOVAL GUERRERO GABRIELA', 'gabriela.sg@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 22, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SUAREZ ALCANTARA LOURDES CRISTELA', 'lourdes.sa@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 20, 12, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('BERMEA DE ITA ARNOLDO', 'arnoldo.bi@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 13, 14, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('CONTRERAS CARRILLO CORINA GUADALUPE', 'corina.cc@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 22, 14, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('RAMOS GARCIA GUILIBALDO', 'guilibaldo.rg@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 2, 2, 14, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('VEGA CASTILLO ABRIL', 'abril.vc@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 25, 14, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('CAVAZOS SANCHEZ EDITH ALICIA', 'edith.cs@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 15, 15, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MARTINEZ RAMIREZ PAULA CECILIA', 'paula.mr@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 19, 16, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SANCHEZ ESPARZA CESAR EDUARDO', 'eduardo.se@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 23, 16, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('ESCOBEDO BRIONES PEDRO GUSTAVO', 'pedro.eb@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 24, 17, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('LOZANO HERNANDEZ CESAR AURELIO', 'cesar.lh@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 8, 17, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MARTINEZ CAMPOS JAVIER', 'javier.mc@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 2, 5, 17, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SANSON OLGUIN MIGUEL ALEJANDRO', 'miguel.so@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 7, 17, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('CASTELLANOS GARCIA JESSICA', 'jessica.cg@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 19, 18, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('GARCIA CONTRERAS KEREN ALEJANDRA', 'keren.gc@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 8, 19, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('ZUÑIGA CASTELLANOS ADRIANA IVETTH', 'adriana.zc@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 22, 19, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('ESTRADA DE LOS SANTOS VICTOR HUGO', 'victor.es@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 27, 20, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('GUZMAN RIVAS JAKZIR', 'jakzir.gr@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 3, 9, 20, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('VALLEJO CORTES MARICELA', 'maricela.vc@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 21, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('ALMAZO HERNANDEZ ISIDRO PETRONILO', 'isidro.ah@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 21, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SALDUA MEDINA ANA LAURA', 'analaura.sm@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 4, 28, 21, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('ARCE ORTIZ FIDENCIO', 'fidencio.ao@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 21, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('LUGO RAMOS ELIZABETH', 'elizabeth.lr@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 21, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('MATA SALINAS ADRIAN JONAHTAN', 'adrian.ms@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 21, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SOLIS CASTILLO LAURA IMELDA', 'laura.sc@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 08:22:32', NULL, 1, 4, 1, 19, 21, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('SALINAS MENDOZA ALFREDO', 'alfredo.salinas@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-21 19:20:24', NULL, 1, 2, 1, 1, 1, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('Axel Ruiz', 'axel.ruiz@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-22 11:44:03', NULL, 1, 1, 1, 1, 1, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('FARIAS MARTINEZ RAUL SERGIO', 'sergio.farias@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-22 11:59:58', NULL, 1, 2, 2, 1, 1, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('LIÑAN TORRES RAFAEL', 'rafael.linan@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-01-23 10:36:59', NULL, 1, 2, 3, 4, 15, 1, NULL, NULL);
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, FechaModificacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea, IdUsuarioModifica, IdApiKey) VALUES ('DE LA GARZA DE LUNA JESUS ROBERTO', 'jesus.delagarza@monclova.tecnm.mx', '4183B9F5ED14B64D012CE1E728CFA1E7AFC399CB82B6729B222784DB6B1A50A7', '2026-02-06 11:18:19', NULL, 1, 2, 4, 1, 1, 1, NULL, NULL);

-- =============================================
-- PASO 6: Tablas restantes
-- =============================================
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('GET', '2026-01-19 03:46:30', NULL, 1, 1, NULL);
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('POST', '2026-01-19 03:46:30', NULL, 1, 1, NULL);
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('PUT', '2026-01-19 03:46:30', NULL, 1, 1, NULL);
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('DELETE FISICO', '2026-01-19 03:46:30', NULL, 1, 1, NULL);
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('PATCH', '2026-01-19 03:46:30', NULL, 1, 1, NULL);
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('HEAD', '2026-01-19 03:46:30', NULL, 1, 1, NULL);
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('OPTIONS', '2026-01-19 03:46:30', NULL, 1, 1, NULL);
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('DELETE LOGICO', '2026-01-22 19:44:07', NULL, 1, 1, NULL);

INSERT INTO ApiPermiso (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Leer', '2026-01-21 19:27:22', NULL, 1, 1, NULL);
INSERT INTO ApiPermiso (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Escribir', '2026-01-21 19:27:22', NULL, 1, 1, NULL);
INSERT INTO ApiPermiso (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Actualizar', '2026-01-21 19:27:22', NULL, 1, 1, NULL);
INSERT INTO ApiPermiso (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Eliminar', '2026-01-21 19:27:22', NULL, 1, 1, NULL);

INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ApiFunctions', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ApiKey', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ApiPermiso', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ApiPermisosApiKeysTabla', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Area', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Corte', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Departamento', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Permiso', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Puesto', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Tabla', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('TipoPermiso', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('TipoUsuario', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Usuario', '2026-01-19 04:02:09', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('ResetPasswordTokens', '2026-01-22 18:40:10', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Auditoria', '2026-01-22 19:44:23', NULL, 1, 1, NULL);
INSERT INTO Tabla (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('TipoOrigen', '2026-02-09 13:07:18', NULL, 1, 1, NULL);

INSERT INTO TipoOrigen (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Interfaz Web', '2026-02-09 12:59:19', NULL, 1, 1, NULL);
INSERT INTO TipoOrigen (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('API Externa', '2026-02-09 12:59:19', NULL, 1, 1, NULL);
INSERT INTO TipoOrigen (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Sistema Automático', '2026-02-09 12:59:19', NULL, 1, 1, NULL);
INSERT INTO TipoOrigen (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Job Programado', '2026-02-09 12:59:19', NULL, 1, 1, NULL);
INSERT INTO TipoOrigen (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Sincronización LDAP', '2026-02-09 12:59:19', NULL, 1, 1, NULL);
INSERT INTO TipoOrigen (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Migración de Datos', '2026-02-09 12:59:19', NULL, 1, 1, NULL);

INSERT INTO TipoPermiso (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Retardo', '2026-01-18 15:03:34', NULL, 1, 1, NULL);
INSERT INTO TipoPermiso (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Turno por Turno', '2026-01-18 15:03:34', NULL, 1, 1, NULL);
INSERT INTO TipoPermiso (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Falta', '2026-01-18 15:03:34', NULL, 1, 1, NULL);
INSERT INTO TipoPermiso (Descripcion, FechaCreacion, FechaModificacion, Estatus, IdUsuarioCrea, IdUsuarioModifica) VALUES ('Cambio de Horario', '2026-01-18 15:03:34', NULL, 1, 1, NULL);

-- =============================================
-- REACTIVAR FK
-- =============================================
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
