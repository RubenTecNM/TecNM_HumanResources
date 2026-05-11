-- =============================================================================
-- Script de Poblado Inicial Consolidad - TecRHPermisos
-- Este script es ejecutado por Program.cs al detectar una base de datos vacía.
-- Contiene Catálogos Base, Áreas, Departamentos, Puestos y Usuarios de prueba.
-- =============================================================================

-- 1. DESHABILITAR RESTRICCIONES FK (Necesario para la auto-creación del usuario root)
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- 2. INSERCIÓN DE CATÁLOGOS BASE

-- TipoUsuario
INSERT INTO TipoUsuario (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Administrador', GETDATE(), 1, 1),
('Autorizador', GETDATE(), 1, 1),
('Recursos Humanos', GETDATE(), 1, 1),
('Trabajador', GETDATE(), 1, 1);

-- TipoEstadoCorte
INSERT INTO TipoEstadoCorte (Descripcion, Color, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Abierto', 'success', GETDATE(), 1, 1),
('Cerrado', 'secondary', GETDATE(), 1, 1),
('Invalidado', 'danger', GETDATE(), 1, 1);

-- TipoEstadoPermiso
INSERT INTO TipoEstadoPermiso (Descripcion, Color, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Pendiente por Revisar', 'warning', GETDATE(), 1, 1),
('Revisado / Aprobado', 'success', GETDATE(), 1, 1),
('Invalidado por Periodo', 'danger', GETDATE(), 1, 1),
('Rechazado', 'dark', GETDATE(), 1, 1);

-- Area
INSERT INTO Area (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('SUBDIRECCION DE SERVICIOS ADMINISTRATIVOS', GETDATE(), 1, 1),
('DIRECCION GENERAL', GETDATE(), 1, 1),
('DIRECCION DE PLANEACION Y VINCULACION', GETDATE(), 1, 1),
('DIRECCION ACADEMICA', GETDATE(), 1, 1);

-- Departamento
INSERT INTO Departamento (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('ACTIVIDADES EXTRAESCOLARES', GETDATE(), 1, 1),
('ACTIVO FIJO', GETDATE(), 1, 1),
('ASUNTOS ACADÉMICOS', GETDATE(), 1, 1),
('BIBLIOTECA', GETDATE(), 1, 1),
('CENTRO DE CÓMPUTO', GETDATE(), 1, 1),
('CENTRO DE ENTRENAMIENTO NAUTILUS', GETDATE(), 1, 1),
('CIE', GETDATE(), 1, 1),
('COMPRAS', GETDATE(), 1, 1),
('COMUNICACIÓN', GETDATE(), 1, 1),
('CONTROL INTERNO', GETDATE(), 1, 1),
('COORDINACIÓN ACADÉMICA', GETDATE(), 1, 1),
('COORDINACIÓN DE INNOVACIÓN EDUCATIVA', GETDATE(), 1, 1),
('DEPARTAMENTO JURÍDICO', GETDATE(), 1, 1),
('DIRECCION GENERAL', GETDATE(), 1, 1),
('ENFERMERÍA', GETDATE(), 1, 1),
('GESTIÓN TECNOLÓGICA Y VINCULACIÓN', GETDATE(), 1, 1),
('IDIOMAS', GETDATE(), 1, 1),
('INNOVACIÓN Y CALIDAD', GETDATE(), 1, 1),
('MANTENIMIENTO', GETDATE(), 1, 1),
('PLANEACIÓN Y PRESUPUESTO', GETDATE(), 1, 1),
('PREFECTURA', GETDATE(), 1, 1),
('PSICOLOGÍA', GETDATE(), 1, 1),
('RECURSOS FINANCIEROS', GETDATE(), 1, 1),
('RECURSOS HUMANOS', GETDATE(), 1, 1),
('SERVICIOS ESCOLARES', GETDATE(), 1, 1),
('SUBDIRECCIÓN DE SERVICIOS', GETDATE(), 1, 1),
('TALLER DE LIT', GETDATE(), 1, 1),
('TRABAJO SOCIAL', GETDATE(), 1, 1);

-- Puesto
INSERT INTO Puesto (Descripcion, ClaveTabular, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('ALMACENISTA', 'A03004', GETDATE(), 1, 1),
('ANALISTA ESPECIALIZADO(A)', 'P01002', GETDATE(), 1, 1),
('ANALISTA TECNICO(A)', 'P01001', GETDATE(), 1, 1),
('AUXILIAR ADMINISTRATIVO', 'A01024', GETDATE(), 1, 1),
('BIBLIOTECARIO(A)', 'T05003', GETDATE(), 1, 1),
('CAPTURISTA', 'T06027', GETDATE(), 1, 1),
('CHOFER DE DIRECTOR', 'CF53453', GETDATE(), 1, 1),
('INGENIERO EN SISTEMAS', 'CF12027', GETDATE(), 1, 1),
('INTENDENTE', 'S06002', GETDATE(), 1, 1),
('JEFE DE DIVISION', 'ITS0003', GETDATE(), 1, 1),
('JEFE DE OFICINA', 'A01001', GETDATE(), 1, 1),
('JEFE(A) DE DEPARTAMENTO', 'ITS0004', GETDATE(), 1, 1),
('LABORATORISTA', 'T16005', GETDATE(), 1, 1),
('MEDICO GENERAL', 'P13006', GETDATE(), 1, 1),
('PROFESOR ASIGNATURA A', 'E13001', GETDATE(), 1, 1),
('PROFESOR ASIGNATURA B', 'E13002', GETDATE(), 1, 1),
('PROFESOR ASOCIADO A', 'E13010', GETDATE(), 1, 1),
('SECRETARIA DE DIRECTOR GENERAL', 'CF53455', GETDATE(), 1, 1),
('SECRETARIA DE SUBDIRECTOR', 'CF34280', GETDATE(), 1, 1),
('SECRETARIO(A) DE JEFE(A) DE DEPARTAMENTO', 'CF34004', GETDATE(), 1, 1),
('TAQUIMECANOGRAFA', 'A08004', GETDATE(), 1, 1),
('TECNICO EN MANTENIMIENTO', 'S08011', GETDATE(), 1, 1),
('TECNICO ESPECIALIZADO', 'CF33116', GETDATE(), 1, 1),
('VIGILANTE', 'S14001', GETDATE(), 1, 1);

-- 3. USUARIOS DE PRUEBA
-- Clave por defecto para todos: Buss1ness.2026
-- Hash SHA256: 5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7

-- ROOT
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea) 
VALUES ('root', 'controlpermisos@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 1, 1, 1, 1, 1);

-- PERSONAL DE PRUEBA
INSERT INTO Usuario (Nombre, Correo, Clave, FechaCreacion, Estatus, IdTipoUsuario, IdArea, IdDepartamento, IdPuesto, IdUsuarioCrea) VALUES 
('RODRIGUEZ HERNANDEZ FRANCISCO', 'francisco.rh@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 1, 2, 19, 1, 1),
('MONTOYA ELIZONDO MAIRA DE JESUS', 'maira.me@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 2, 2, 5, 2, 1),
('AGUILERA MANCILLA DANIELA', 'daniela.am@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 26, 3, 1),
('RIVERA QUINTERO OLGA LUCINDA', 'olga.rq@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 23, 3, 1),
('VILLANUEVA FLORES MAYRA', 'mayra.vf@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 2, 14, 3, 1),
('OZUNA RODRIGUEZ RAQUEL', 'raquel.or@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 21, 4, 1),
('VAZQUEZ QUINTERO JUAN', 'juan.vq@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 15, 4, 1),
('GONZALEZ FLORES LAURA ISELDA', 'laura.gf@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 25, 5, 1),
('CAMPOS RODRIGUEZ LETICIA ABIGAIL', 'leticia.cr@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 25, 6, 1),
('LOERA SOLIS JUAN FRANCISCO', 'francisco.ls@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 9, 6, 1),
('LOZANO FERNANDEZ JESUS FERNANDO', 'fernando.lf@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 21, 6, 1),
('MENDEZ HERNANDEZ IZAMAR GUADALUPE', 'izamar.mh@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 3, 1, 24, 6, 1),
('PECINA VAZQUEZ ELSA MARICELA', 'elsa.pv@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 4, 6, 1),
('RAMIREZ GOMEZ RAQUEL DODANIM', 'raquel.rg@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 23, 6, 1),
('SALOMON TADEO NICOLAS', 'nicolas.st@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 7, 1),
('ROMO VEGA CESAR ALBERTO', 'cesar.rv@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 27, 8, 1),
('DE LEON REYNA JOSE ALFREDO', 'jose.lr@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 9, 1),
('DE LEON ZAMORA JUAN MANUEL', 'juan.lz@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 9, 1),
('ESCOBEDO GUTIERREZ CLAUDIO JESUS', 'claudio.eg@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 9, 1),
('GUTIERREZ REYES LUZ ELENA', 'luz.gr@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 9, 1),
('REYNA CASTILLO SAN JUANA', 'sanjuana.rc@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 9, 1),
('BORJAS LEIJA MARIA TERESA', 'teresa.bl@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 12, 10, 1),
('DE LA CRUZ MATA MARTINA', 'martina.cm@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 17, 10, 1),
('MENDOZA RIOJAS ROCIO DEL CARMEN', 'rocio.mendoza@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 16, 10, 1),
('SALOMON TADEO MARIA ANASTACIO', 'maria.st@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 21, 10, 1),
('SANDOVAL GUERRERO ROSALVA', 'rosalva.sg@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 3, 10, 1),
('VAZQUEZ SALINAS CLAUDIA', 'claudia.vs@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 25, 10, 1),
('DE LEON MARTINEZ ANA CRISTINA', 'ana.lm@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 26, 11, 1),
('GONZALEZ PALOALTO MARGARITA', 'margarita.gp@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 11, 11, 1),
('HERNANDEZ DE LA GARZA VERONICA PATRICIA', 'veronica.hg@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 25, 11, 1),
('CASTRO MARTINEZ GRACIELA', 'graciela.cm@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 18, 12, 1),
('CHAIREZ HUERTA SERGIO IVAN', 'ivan.chairez@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 2, 2, 12, 1),
('DE LA ROSA MARES JOEL', 'joel.rm@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 1, 12, 1),
('GALARZA RINCON TELMA CECILIA', 'telma.gr@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 10, 12, 1),
('GUARDIOLA NUÑEZ JAVIER JULIAN', 'javier.gn@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 12, 1),
('MARTINEZ DE HOYOS ARTURO', 'arturo.martinez@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 16, 12, 1),
('MENDOZA VILLA FIDENCIO ALBERTO', 'fidencio.mendoza@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 2, 5, 12, 1),
('MUÑOZ DOMINGUEZ ROSARIO IRENE', 'rosario.md@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 23, 12, 1),
('RAMOS GARCIA JUANITA GUADALUPE', 'juanita.rg@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 24, 12, 1),
('ROMO HERRERA JESUS', 'jesus.rh@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 6, 12, 1),
('SANDOVAL GUERRERO GABRIELA', 'gabriela.sg@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 22, 12, 1),
('SUAREZ ALCANTARA LOURDES CRISTELA', 'lourdes.sa@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 20, 12, 1),
('BERMEA DE ITA ARNOLDO', 'arnoldo.bi@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 13, 14, 1),
('CONTRERAS CARRILLO CORINA GUADALUPE', 'corina.cc@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 22, 14, 1),
('RAMOS GARCIA GUILIBALDO', 'guilibaldo.rg@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 2, 2, 14, 1),
('VEGA CASTILLO ABRIL', 'abril.vc@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 25, 14, 1),
('CAVAZOS SANCHEZ EDITH ALICIA', 'edith.cs@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 15, 15, 1),
('MARTINEZ RAMIREZ PAULA CECILIA', 'paula.mr@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 19, 16, 1),
('SANCHEZ ESPARZA CESAR EDUARDO', 'eduardo.se@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 23, 16, 1),
('ESCOBEDO BRIONES PEDRO GUSTAVO', 'pedro.eb@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 24, 17, 1),
('LOZANO HERNANDEZ CESAR AURELIO', 'cesar.lh@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 8, 17, 1),
('MARTINEZ CAMPOS JAVIER', 'javier.mc@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 2, 5, 17, 1),
('SANSON OLGUIN MIGUEL ALEJANDRO', 'miguel.so@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 7, 17, 1),
('CASTELLANOS GARCIA JESSICA', 'jessica.cg@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 19, 18, 1),
('GARCIA CONTRERAS KEREN ALEJANDRA', 'keren.gc@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 8, 19, 1),
('ZUÑIGA CASTELLANOS ADRIANA IVETTH', 'adriana.zc@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 22, 19, 1),
('ESTRADA DE LOS SANTOS VICTOR HUGO', 'victor.es@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 27, 20, 1),
('GUZMAN RIVAS JAKZIR', 'jakzir.gr@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 3, 9, 20, 1),
('VALLEJO CORTES MARICELA', 'maricela.vc@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 21, 1),
('ALMAZO HERNANDEZ ISIDRO PETRONILO', 'isidro.ah@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 21, 1),
('SALDUA MEDINA ANA LAURA', 'analaura.sm@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 4, 28, 21, 1),
('ARCE ORTIZ FIDENCIO', 'fidencio.ao@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 21, 1),
('LUGO RAMOS ELIZABETH', 'elizabeth.lr@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 21, 1),
('MATA SALINAS ADRIAN JONAHTAN', 'adrian.ms@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 21, 1),
('SOLIS CASTILLO LAURA IMELDA', 'laura.sc@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 4, 1, 19, 21, 1),
('SALINAS MENDOZA ALFREDO', 'alfredo.salinas@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 2, 1, 1, 1, 1),
('Axel Ruiz', 'axel.ruiz@monclova.tecnm.mx', '5166099b80693a74112e52e935a8bc762699e1ac74d4719b4cfb088e894c25f7', GETDATE(), 1, 1, 1, 1, 1, 1);

-- 4. CONFIGURACIÓN DE TABLAS DE SISTEMA Y API

-- ApiFunctions
INSERT INTO ApiFunctions (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('GET', GETDATE(), 1, 1),
('POST', GETDATE(), 1, 1),
('PUT', GETDATE(), 1, 1),
('DELETE FISICO', GETDATE(), 1, 1),
('PATCH', GETDATE(), 1, 1),
('HEAD', GETDATE(), 1, 1),
('OPTIONS', GETDATE(), 1, 1),
('DELETE LOGICO', GETDATE(), 1, 1);

-- ApiPermiso
INSERT INTO ApiPermiso (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Leer', GETDATE(), 1, 1),
('Escribir', GETDATE(), 1, 1),
('Actualizar', GETDATE(), 1, 1),
('Eliminar', GETDATE(), 1, 1);

-- Tabla
INSERT INTO Tabla (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('ApiFunctions', GETDATE(), 1, 1),
('ApiKey', GETDATE(), 1, 1),
('ApiPermiso', GETDATE(), 1, 1),
('ApiPermisosApiKeysTabla', GETDATE(), 1, 1),
('Area', GETDATE(), 1, 1),
('Corte', GETDATE(), 1, 1),
('Departamento', GETDATE(), 1, 1),
('Permiso', GETDATE(), 1, 1),
('Puesto', GETDATE(), 1, 1),
('Tabla', GETDATE(), 1, 1),
('TipoPermiso', GETDATE(), 1, 1),
('TipoUsuario', GETDATE(), 1, 1),
('Usuario', GETDATE(), 1, 1),
('ResetPasswordTokens', GETDATE(), 1, 1),
('Auditoria', GETDATE(), 1, 1),
('TipoOrigen', GETDATE(), 1, 1),
('TipoEstadoCorte', GETDATE(), 1, 1),
('TipoEstadoPermiso', GETDATE(), 1, 1);

-- TipoOrigen
INSERT INTO TipoOrigen (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Interfaz Web', GETDATE(), 1, 1),
('API Externa', GETDATE(), 1, 1),
('Sistema Automático', GETDATE(), 1, 1),
('Job Programado', GETDATE(), 1, 1),
('Sincronización LDAP', GETDATE(), 1, 1),
('Migración de Datos', GETDATE(), 1, 1);

-- TipoPermiso
INSERT INTO TipoPermiso (Descripcion, FechaCreacion, Estatus, IdUsuarioCrea) VALUES 
('Retardo', GETDATE(), 1, 1),
('Turno por Turno', GETDATE(), 1, 1),
('Falta', GETDATE(), 1, 1),
('Cambio de Horario', GETDATE(), 1, 1);

-- 5. REACTIVAR FK
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO
