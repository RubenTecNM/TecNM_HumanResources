-- Esquema completo de la base de datos RecursosHumanos
-- Generado desde dockerRH (MSSQL Server en Docker)
CREATE DATABASE RecursosHumanos;
GO

USE RecursosHumanos;
GO

CREATE TABLE [ApiFunctions] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(50) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [ApiKey] ([Id] int IDENTITY(1,1) NOT NULL, [Clave] varchar(64) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [ApiPermiso] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(255) NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [ApiPermisosApiKeysTabla] ([Id] int IDENTITY(1,1) NOT NULL, [IdApiKey] int NOT NULL, [IdApiPermiso] int NOT NULL, [IdTabla] int NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [Area] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(50) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [Auditoria] ([Id] int IDENTITY(1,1) NOT NULL, [IdTabla] int NOT NULL, [IdRegistro] int NOT NULL, [IdApiFunction] int NOT NULL, [IdApiKey] int NULL, [DatosAnteriores] nvarchar(MAX) NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, [IdTipoOrigen] int NOT NULL, PRIMARY KEY ([Id]));

CREATE TABLE [Corte] ([Id] int IDENTITY(1,1) NOT NULL, [Inicia] datetime NOT NULL, [Termina] datetime NOT NULL, [FechaCreacion] datetime2 NOT NULL, [FechaModificacion] datetime2 NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [Departamento] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(50) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [Permiso] ([Id] int IDENTITY(1,1) NOT NULL, [Fecha1] datetime NULL, [Fecha2] datetime NULL, [Dias] int NULL, [Motivo] varchar(255) NOT NULL, [Evidencia] varchar(255) NULL, [FechaAutorizacion] datetime2 NULL, [FechaCreacion] datetime2 NOT NULL, [FechaModificacion] datetime2 NULL, [Revisado] bit NOT NULL, [Goce] bit NOT NULL, [Estatus] bit NOT NULL, [IdCorte] int NOT NULL, [IdTipoPermiso] int NOT NULL, [IdUsuarioSolicita] int NOT NULL, [IdUsuarioAutoriza] int NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [Puesto] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(50) NOT NULL, [ClaveTabular] varchar(10) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [ResetPasswordTokens] ([Id] int IDENTITY(1,1) NOT NULL, [Selector] varchar(255) NOT NULL, [TokenHash] varchar(255) NOT NULL, [Utilizado] bit NOT NULL, [IdUsuarioSolicita] int NOT NULL, [IdUsuarioCrea] int NULL, [IdUsuarioModifica] int NULL, [Estatus] bit NOT NULL, [FechaModificacion] datetime NULL, [FechaCreacion] datetime NOT NULL, [FechaCaducidad] datetime NOT NULL, PRIMARY KEY ([Id]));

CREATE TABLE [Tabla] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(255) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [TipoOrigen] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(64) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [TipoPermiso] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(50) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [TipoUsuario] ([Id] int IDENTITY(1,1) NOT NULL, [Descripcion] varchar(50) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, PRIMARY KEY ([Id]));

CREATE TABLE [Usuario] ([Id] int IDENTITY(1,1) NOT NULL, [Nombre] varchar(100) NOT NULL, [Correo] varchar(150) NOT NULL, [Clave] varchar(256) NOT NULL, [FechaCreacion] datetime NOT NULL, [FechaModificacion] datetime NULL, [Estatus] bit NOT NULL, [IdTipoUsuario] int NOT NULL, [IdArea] int NOT NULL, [IdDepartamento] int NOT NULL, [IdPuesto] int NOT NULL, [IdUsuarioCrea] int NOT NULL, [IdUsuarioModifica] int NULL, [IdApiKey] int NULL, PRIMARY KEY ([Id]));

-- Foreign Key Constraints
ALTER TABLE [ApiFunctions] ADD CONSTRAINT [FK_11] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [ApiFunctions] ADD CONSTRAINT [FK_12] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [ApiKey] ADD CONSTRAINT [FK_7] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [ApiKey] ADD CONSTRAINT [FK_8] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [ApiPermiso] ADD CONSTRAINT [FK_1] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [ApiPermiso] ADD CONSTRAINT [FK_2] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [ApiPermisosApiKeysTabla] ADD CONSTRAINT [FK_14] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [ApiPermisosApiKeysTabla] ADD CONSTRAINT [FK_15] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [ApiPermisosApiKeysTabla] ADD CONSTRAINT [FK_23] FOREIGN KEY ([IdApiKey]) REFERENCES [ApiKey] ([Id]);
ALTER TABLE [ApiPermisosApiKeysTabla] ADD CONSTRAINT [FK_24] FOREIGN KEY ([IdTabla]) REFERENCES [Tabla] ([Id]);
ALTER TABLE [ApiPermisosApiKeysTabla] ADD CONSTRAINT [FK_25] FOREIGN KEY ([IdApiPermiso]) REFERENCES [ApiPermiso] ([Id]);

ALTER TABLE [Area] ADD CONSTRAINT [FK_6] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Area] ADD CONSTRAINT [FK_Area_UsuarioCrea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_ApiFunction] FOREIGN KEY ([IdApiFunction]) REFERENCES [ApiFunctions] ([Id]);
ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_ApiKey] FOREIGN KEY ([IdApiKey]) REFERENCES [ApiKey] ([Id]);
ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_Tabla] FOREIGN KEY ([IdTabla]) REFERENCES [Tabla] ([Id]);
ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_TipoOrigen] FOREIGN KEY ([IdTipoOrigen]) REFERENCES [TipoOrigen] ([Id]);
ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_UsuarioCrea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_UsuarioModifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [Corte] ADD CONSTRAINT [FK_19] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Corte] ADD CONSTRAINT [FK_20] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [Departamento] ADD CONSTRAINT [FK_3] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Departamento] ADD CONSTRAINT [FK_Depto_UsuarioCrea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [Permiso] ADD CONSTRAINT [FK_16] FOREIGN KEY ([IdUsuarioAutoriza]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Permiso] ADD CONSTRAINT [FK_17] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Permiso] ADD CONSTRAINT [FK_18] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Permiso] ADD CONSTRAINT [FK_Permiso_Corte] FOREIGN KEY ([IdCorte]) REFERENCES [Corte] ([Id]);
ALTER TABLE [Permiso] ADD CONSTRAINT [FK_Permiso_Tipo] FOREIGN KEY ([IdTipoPermiso]) REFERENCES [TipoPermiso] ([Id]);
ALTER TABLE [Permiso] ADD CONSTRAINT [FK_Permiso_UsuarioSolicita] FOREIGN KEY ([IdUsuarioSolicita]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [Puesto] ADD CONSTRAINT [FK_13] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Puesto] ADD CONSTRAINT [FK_Puesto_UsuarioCrea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [ResetPasswordTokens] ADD CONSTRAINT [FK_ResetPasswordTokens_UsuarioCrea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [ResetPasswordTokens] ADD CONSTRAINT [FK_ResetPasswordTokens_UsuarioModifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [ResetPasswordTokens] ADD CONSTRAINT [FK_ResetPasswordTokens_UsuarioSolicita] FOREIGN KEY ([IdUsuarioSolicita]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [Tabla] ADD CONSTRAINT [FK_10] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Tabla] ADD CONSTRAINT [FK_9] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [TipoOrigen] ADD CONSTRAINT [FK_TipoOrigen_Usuario_Crea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [TipoOrigen] ADD CONSTRAINT [FK_TipoOrigen_Usuario_Modifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [TipoPermiso] ADD CONSTRAINT [FK_21] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [TipoPermiso] ADD CONSTRAINT [FK_22] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [TipoUsuario] ADD CONSTRAINT [FK_4] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [TipoUsuario] ADD CONSTRAINT [FK_5] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);

ALTER TABLE [Usuario] ADD CONSTRAINT [FK_26] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Usuario] ADD CONSTRAINT [FK_27] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
ALTER TABLE [Usuario] ADD CONSTRAINT [FK_28] FOREIGN KEY ([IdApiKey]) REFERENCES [ApiKey] ([Id]);
ALTER TABLE [Usuario] ADD CONSTRAINT [FK_Usuario_Area] FOREIGN KEY ([IdArea]) REFERENCES [Area] ([Id]);
ALTER TABLE [Usuario] ADD CONSTRAINT [FK_Usuario_Departamento] FOREIGN KEY ([IdDepartamento]) REFERENCES [Departamento] ([Id]);
ALTER TABLE [Usuario] ADD CONSTRAINT [FK_Usuario_Puesto] FOREIGN KEY ([IdPuesto]) REFERENCES [Puesto] ([Id]);
ALTER TABLE [Usuario] ADD CONSTRAINT [FK_Usuario_TipoUsuario] FOREIGN KEY ([IdTipoUsuario]) REFERENCES [TipoUsuario] ([Id]);