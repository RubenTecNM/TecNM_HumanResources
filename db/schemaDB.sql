CREATE TABLE [LoginResultDTO] (
    [Id] int NOT NULL,
    [Nombre] nvarchar(max) NOT NULL,
    [Correo] nvarchar(max) NOT NULL,
    [IdArea] int NOT NULL,
    [NombreArea] nvarchar(max) NOT NULL,
    [IdTipoUsuario] int NOT NULL
);
GO


CREATE TABLE [ResetPasswordTokens] (
    [Id] int NOT NULL IDENTITY,
    [Selector] varchar(255) NOT NULL,
    [TokenHash] varchar(255) NOT NULL,
    [Utilizado] bit NOT NULL DEFAULT CAST(0 AS bit),
    [IdUsuarioSolicita] int NOT NULL,
    [IdUsuarioCrea] int NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaCaducidad] datetime2 NOT NULL,
    CONSTRAINT [PK_ResetPasswordTokens] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [SearchPermissionsDTO] (
    [Id] int NOT NULL,
    [Fecha1] datetime2 NULL,
    [Fecha2] datetime2 NULL,
    [Dias] int NULL,
    [Motivo] nvarchar(max) NOT NULL,
    [Evidencia] nvarchar(max) NULL,
    [FechaAutorizacion] datetime2 NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [FechaModificacion] datetime2 NULL,
    [Revisado] bit NOT NULL,
    [Goce] bit NOT NULL,
    [Estatus] bit NOT NULL,
    [IdCorte] int NOT NULL,
    [IdTipoPermiso] int NOT NULL,
    [IdUsuarioSolicita] int NOT NULL,
    [IdUsuarioAutoriza] int NULL,
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL,
    [Nombre] nvarchar(max) NOT NULL,
    [Correo] nvarchar(max) NOT NULL,
    [TipoPermiso] nvarchar(max) NOT NULL,
    [Area] nvarchar(max) NOT NULL,
    [AutorizadoPor] nvarchar(max) NULL,
    [CreadoPor] nvarchar(max) NOT NULL,
    [ModificadoPor] nvarchar(max) NULL
);
GO


CREATE TABLE [VwPermisosUsuarios] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] nvarchar(max) NULL,
    [DiasSolicitados] int NULL,
    [ConGoce] bit NOT NULL,
    [HoraEntrada] time NULL,
    [HorarioCubrir] time NULL,
    [Evidencia] nvarchar(max) NULL,
    [DelDia] date NULL,
    [AlDia] date NULL,
    [FechaCreacion] datetime2 NULL,
    [FechaSolicitud] datetime2 NULL,
    [FechaModificacion] datetime2 NULL,
    [FechaAutorizacion] datetime2 NULL,
    [IdTipoPermisos] int NOT NULL,
    [IdCorte] int NOT NULL,
    [Estatus] bit NOT NULL,
    [IdUsuarioSolicitud] int NOT NULL,
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NOT NULL,
    [IdUsuarioAutorizador] int NULL,
    [NombreUsuario] nvarchar(max) NOT NULL,
    [Correo] nvarchar(max) NOT NULL,
    [IdArea] int NOT NULL,
    CONSTRAINT [PK_VwPermisosUsuarios] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ApiFunctions] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(50) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__ApiFunct__3214EC0745513D7B] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ApiKey] (
    [Id] int NOT NULL IDENTITY,
    [Clave] varchar(64) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__ApiKey__3214EC07C3DAD720] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ApiPermiso] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(255) NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__ApiPermi__3214EC07EB278C1E] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ApiPermisosApiKeysTabla] (
    [Id] int NOT NULL IDENTITY,
    [IdApiKey] int NOT NULL,
    [IdApiPermiso] int NOT NULL,
    [IdTabla] int NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__ApiPermi__3214EC078F3D9269] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApiPermisosApiKeysTablas_ApiKeys] FOREIGN KEY ([IdApiKey]) REFERENCES [ApiKey] ([Id]),
    CONSTRAINT [FK_ApiPermisosApiKeysTablas_ApiPermisos] FOREIGN KEY ([IdApiPermiso]) REFERENCES [ApiPermiso] ([Id])
);
GO


CREATE TABLE [Area] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(50) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__Area__3214EC07A750D956] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Auditoria] (
    [Id] int NOT NULL IDENTITY,
    [IdTabla] int NOT NULL,
    [IdRegistro] int NOT NULL,
    [IdApiFunction] int NOT NULL,
    [IdApiKey] int NULL,
    [IdTipoOrigen] int NOT NULL,
    [DatosAnteriores] nvarchar(max) NULL,
    [FechaCreacion] datetime2 NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime2 NULL,
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL,
    CONSTRAINT [PK_Auditoria] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Auditoria_ApiFunction] FOREIGN KEY ([IdApiFunction]) REFERENCES [ApiFunctions] ([Id]),
    CONSTRAINT [FK_Auditoria_ApiKey] FOREIGN KEY ([IdApiKey]) REFERENCES [ApiKey] ([Id])
);
GO


CREATE TABLE [Corte] (
    [Id] int NOT NULL IDENTITY,
    [Inicia] datetime NOT NULL,
    [Termina] datetime NOT NULL,
    [FechaCreacion] datetime2(0) NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime2(0) NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__Corte__3214EC07702F01F4] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Departamento] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(50) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__Departam__3214EC071FFCF6A1] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Permiso] (
    [Id] int NOT NULL IDENTITY,
    [Fecha1] datetime NULL,
    [Fecha2] datetime NULL,
    [Dias] int NULL,
    [Motivo] varchar(255) NOT NULL,
    [Evidencia] varchar(255) NULL,
    [FechaAutorizacion] datetime2(0) NULL DEFAULT ((NULL)),
    [FechaCreacion] datetime2(0) NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime2(0) NULL DEFAULT ((NULL)),
    [Revisado] bit NOT NULL,
    [Goce] bit NOT NULL,
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdCorte] int NOT NULL,
    [IdTipoPermiso] int NOT NULL,
    [IdUsuarioSolicita] int NOT NULL,
    [IdUsuarioAutoriza] int NULL,
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__Permiso__3214EC07503FFDCA] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Permiso__IdCorte__2A6B46EF] FOREIGN KEY ([IdCorte]) REFERENCES [Corte] ([Id])
);
GO


CREATE TABLE [Puesto] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(50) NOT NULL,
    [ClaveTabular] varchar(10) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__Puesto__3214EC07319CC249] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Tabla] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(255) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__Tabla__3214EC0771DFB8B8] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [TipoOrigen] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(64) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL,
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL,
    CONSTRAINT [PK_TipoOrigen] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [TipoPermiso] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(50) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__TipoPerm__3214EC07AEB8B6FC] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [TipoUsuario] (
    [Id] int NOT NULL IDENTITY,
    [Descripcion] varchar(50) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__TipoUsua__3214EC070D31226E] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Usuario] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] varchar(100) NOT NULL,
    [Correo] varchar(150) NOT NULL,
    [Clave] varchar(256) NOT NULL,
    [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
    [FechaModificacion] datetime NULL DEFAULT ((NULL)),
    [Estatus] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IdTipoUsuario] int NOT NULL,
    [IdArea] int NOT NULL,
    [IdDepartamento] int NOT NULL,
    [IdPuesto] int NOT NULL,
    [IdUsuarioCrea] int NOT NULL,
    [IdUsuarioModifica] int NULL DEFAULT ((NULL)),
    [IdApiKey] int NULL DEFAULT ((NULL)),
    CONSTRAINT [PK__Usuario__3214EC07981AC028] PRIMARY KEY ([Id]),
    CONSTRAINT [FK__Usuario__IdArea__382F5661] FOREIGN KEY ([IdArea]) REFERENCES [Area] ([Id]),
    CONSTRAINT [FK__Usuario__IdDepar__39237A9A] FOREIGN KEY ([IdDepartamento]) REFERENCES [Departamento] ([Id]),
    CONSTRAINT [FK__Usuario__IdPuest__3A179ED3] FOREIGN KEY ([IdPuesto]) REFERENCES [Puesto] ([Id]),
    CONSTRAINT [FK__Usuario__IdTipoU__3B0BC30C] FOREIGN KEY ([IdTipoUsuario]) REFERENCES [TipoUsuario] ([Id]),
    CONSTRAINT [FK__Usuario__IdUsuar__3BFFE745] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]),
    CONSTRAINT [FK__Usuario__IdUsuar__3CF40B7E] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id])
);
GO


CREATE INDEX [IX_ApiFunctions_IdUsuarioCrea] ON [ApiFunctions] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_ApiFunctions_IdUsuarioModifica] ON [ApiFunctions] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_ApiKey_IdUsuarioCrea] ON [ApiKey] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_ApiKey_IdUsuarioModifica] ON [ApiKey] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_ApiPermiso_IdUsuarioCrea] ON [ApiPermiso] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_ApiPermiso_IdUsuarioModifica] ON [ApiPermiso] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_ApiPermisosApiKeysTabla_IdApiKey] ON [ApiPermisosApiKeysTabla] ([IdApiKey]);
GO


CREATE INDEX [IX_ApiPermisosApiKeysTabla_IdApiPermiso] ON [ApiPermisosApiKeysTabla] ([IdApiPermiso]);
GO


CREATE INDEX [IX_ApiPermisosApiKeysTabla_IdTabla] ON [ApiPermisosApiKeysTabla] ([IdTabla]);
GO


CREATE INDEX [IX_ApiPermisosApiKeysTabla_IdUsuarioCrea] ON [ApiPermisosApiKeysTabla] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_ApiPermisosApiKeysTabla_IdUsuarioModifica] ON [ApiPermisosApiKeysTabla] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_Area_IdUsuarioCrea] ON [Area] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_Area_IdUsuarioModifica] ON [Area] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_Auditoria_IdApiFunction] ON [Auditoria] ([IdApiFunction]);
GO


CREATE INDEX [IX_Auditoria_IdApiKey] ON [Auditoria] ([IdApiKey]);
GO


CREATE INDEX [IX_Auditoria_IdTabla] ON [Auditoria] ([IdTabla]);
GO


CREATE INDEX [IX_Auditoria_IdTipoOrigen] ON [Auditoria] ([IdTipoOrigen]);
GO


CREATE INDEX [IX_Auditoria_IdUsuarioCrea] ON [Auditoria] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_Auditoria_IdUsuarioModifica] ON [Auditoria] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_Corte_IdUsuarioCrea] ON [Corte] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_Corte_IdUsuarioModifica] ON [Corte] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_Departamento_IdUsuarioCrea] ON [Departamento] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_Departamento_IdUsuarioModifica] ON [Departamento] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_Permiso_IdCorte] ON [Permiso] ([IdCorte]);
GO


CREATE INDEX [IX_Permiso_IdTipoPermiso] ON [Permiso] ([IdTipoPermiso]);
GO


CREATE INDEX [IX_Permiso_IdUsuarioAutoriza] ON [Permiso] ([IdUsuarioAutoriza]);
GO


CREATE INDEX [IX_Permiso_IdUsuarioCrea] ON [Permiso] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_Permiso_IdUsuarioModifica] ON [Permiso] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_Permiso_IdUsuarioSolicita] ON [Permiso] ([IdUsuarioSolicita]);
GO


CREATE INDEX [IX_Puesto_IdUsuarioCrea] ON [Puesto] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_Puesto_IdUsuarioModifica] ON [Puesto] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_Tabla_IdUsuarioCrea] ON [Tabla] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_Tabla_IdUsuarioModifica] ON [Tabla] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_TipoOrigen_IdUsuarioCrea] ON [TipoOrigen] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_TipoOrigen_IdUsuarioModifica] ON [TipoOrigen] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_TipoPermiso_IdUsuarioCrea] ON [TipoPermiso] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_TipoPermiso_IdUsuarioModifica] ON [TipoPermiso] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_TipoUsuario_IdUsuarioCrea] ON [TipoUsuario] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_TipoUsuario_IdUsuarioModifica] ON [TipoUsuario] ([IdUsuarioModifica]);
GO


CREATE INDEX [IX_Usuario_IdArea] ON [Usuario] ([IdArea]);
GO


CREATE INDEX [IX_Usuario_IdDepartamento] ON [Usuario] ([IdDepartamento]);
GO


CREATE INDEX [IX_Usuario_IdPuesto] ON [Usuario] ([IdPuesto]);
GO


CREATE INDEX [IX_Usuario_IdTipoUsuario] ON [Usuario] ([IdTipoUsuario]);
GO


CREATE INDEX [IX_Usuario_IdUsuarioCrea] ON [Usuario] ([IdUsuarioCrea]);
GO


CREATE INDEX [IX_Usuario_IdUsuarioModifica] ON [Usuario] ([IdUsuarioModifica]);
GO


ALTER TABLE [ApiFunctions] ADD CONSTRAINT [FK_ApiFunctions_Usuario_Crea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [ApiFunctions] ADD CONSTRAINT [FK_ApiFunctions_Usuario_Modifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [ApiKey] ADD CONSTRAINT [FK_ApiKeys_Usuario_Crea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [ApiKey] ADD CONSTRAINT [FK_ApiKeys_Usuario_Modifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [ApiPermiso] ADD CONSTRAINT [FK_ApiPermisos_Usuario_Crea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [ApiPermiso] ADD CONSTRAINT [FK_ApiPermisos_Usuario_Modifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [ApiPermisosApiKeysTabla] ADD CONSTRAINT [FK_ApiPermisosApiKeysTablas_Tablas] FOREIGN KEY ([IdTabla]) REFERENCES [Tabla] ([Id]);
GO


ALTER TABLE [ApiPermisosApiKeysTabla] ADD CONSTRAINT [FK_ApiPermisosApiKeysTablas_UsuarioCrea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [ApiPermisosApiKeysTabla] ADD CONSTRAINT [FK_ApiPermisosApiKeysTablas_UsuarioModifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Area] ADD CONSTRAINT [FK__Area__IdUsuarioC__24285DB4] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Area] ADD CONSTRAINT [FK__Area__IdUsuarioM__251C81ED] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_Tabla] FOREIGN KEY ([IdTabla]) REFERENCES [Tabla] ([Id]);
GO


ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_TipoOrigen] FOREIGN KEY ([IdTipoOrigen]) REFERENCES [TipoOrigen] ([Id]);
GO


ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_UsuarioCrea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Auditoria] ADD CONSTRAINT [FK_Auditoria_UsuarioModifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Corte] ADD CONSTRAINT [FK__Corte__IdUsuario__74AE54BC] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Corte] ADD CONSTRAINT [FK__Corte__IdUsuario__75A278F5] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Departamento] ADD CONSTRAINT [FK__Departame__IdUsu__27F8EE98] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Departamento] ADD CONSTRAINT [FK__Departame__IdUsu__28ED12D1] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Permiso] ADD CONSTRAINT [FK__Permiso__IdTipoP__2B5F6B28] FOREIGN KEY ([IdTipoPermiso]) REFERENCES [TipoPermiso] ([Id]);
GO


ALTER TABLE [Permiso] ADD CONSTRAINT [FK__Permiso__IdUsuar__2C538F61] FOREIGN KEY ([IdUsuarioSolicita]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Permiso] ADD CONSTRAINT [FK__Permiso__IdUsuar__2D47B39A] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Permiso] ADD CONSTRAINT [FK__Permiso__IdUsuar__2E3BD7D3] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Permiso] ADD CONSTRAINT [FK__Permiso__IdUsuar__2F2FFC0C] FOREIGN KEY ([IdUsuarioAutoriza]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Puesto] ADD CONSTRAINT [FK__Puesto__IdUsuari__308E3499] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Puesto] ADD CONSTRAINT [FK__Puesto__IdUsuari__318258D2] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Tabla] ADD CONSTRAINT [FK_Tablas_Usuario_Crea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [Tabla] ADD CONSTRAINT [FK_Tablas_Usuario_Modifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [TipoOrigen] ADD CONSTRAINT [FK_TipoOrigen_Usuario_Crea] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [TipoOrigen] ADD CONSTRAINT [FK_TipoOrigen_Usuario_Modifica] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [TipoPermiso] ADD CONSTRAINT [FK__TipoPermi__IdUsu__345EC57D] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [TipoPermiso] ADD CONSTRAINT [FK__TipoPermi__IdUsu__3552E9B6] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [TipoUsuario] ADD CONSTRAINT [FK__TipoUsuar__IdUsu__36470DEF] FOREIGN KEY ([IdUsuarioCrea]) REFERENCES [Usuario] ([Id]);
GO


ALTER TABLE [TipoUsuario] ADD CONSTRAINT [FK__TipoUsuar__IdUsu__373B3228] FOREIGN KEY ([IdUsuarioModifica]) REFERENCES [Usuario] ([Id]);
GO


