-- =============================================================================
-- Migration: Rename schema objects from Spanish to English (data-preserving)
-- =============================================================================
-- Runs against databases created with the Spanish schema (e.g. DB_Development_
-- HumanResources, DB_Testing_HumanResources, DB_Production_HumanResources)
-- so the codebase (develop branch) works against them without data loss.
--
-- After running this script, execute schema_objects.sql to (re)create the
-- stored procedures and triggers with their English names.
-- =============================================================================
SET NOCOUNT ON;

-- =============================================================================
-- 1. RENAME TABLES
-- =============================================================================
IF OBJECT_ID(N'dbo.Permiso', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[Permiso]', N'Permission';
IF OBJECT_ID(N'dbo.Usuario', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[Usuario]', N'User';
IF OBJECT_ID(N'dbo.Corte', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[Corte]', N'Cutoff';
IF OBJECT_ID(N'dbo.Departamento', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[Departamento]', N'Department';
IF OBJECT_ID(N'dbo.Puesto', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[Puesto]', N'Position';
IF OBJECT_ID(N'dbo.TipoUsuario', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[TipoUsuario]', N'UserType';
IF OBJECT_ID(N'dbo.TipoPermiso', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[TipoPermiso]', N'PermissionType';
IF OBJECT_ID(N'dbo.TipoEstadoPermiso', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[TipoEstadoPermiso]', N'PermissionStatusType';
IF OBJECT_ID(N'dbo.TipoEstadoCorte', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[TipoEstadoCorte]', N'CutoffStatusType';
IF OBJECT_ID(N'dbo.TipoOrigen', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[TipoOrigen]', N'OriginType';
IF OBJECT_ID(N'dbo.ApiPermiso', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[ApiPermiso]', N'ApiPermission';
IF OBJECT_ID(N'dbo.ApiPermisosApiKeysTabla', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[ApiPermisosApiKeysTabla]', N'ApiKeyTablePermission';
IF OBJECT_ID(N'dbo.Tabla', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[Tabla]', N'DatabaseTable';
IF OBJECT_ID(N'dbo.Auditoria', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[Auditoria]', N'AuditLog';
IF OBJECT_ID(N'dbo.VwPermisosUsuarios', N'U') IS NOT NULL EXEC sp_rename N'[dbo].[VwPermisosUsuarios]', N'VwPermissionsUsers';
IF OBJECT_ID(N'dbo.VwPermisosUsuarios', N'V') IS NOT NULL EXEC sp_rename N'[dbo].[VwPermisosUsuarios]', N'VwPermissionsUsers';
GO
-- Area, ApiKey, ApiFunction and ResetPasswordToken keep their names.

-- =============================================================================
-- 2. RENAME COMMON (BaseEntity) COLUMNS
-- =============================================================================
IF COL_LENGTH(N'dbo.Permission', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.[User]', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[User].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[User].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[User].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[User].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[User].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.Cutoff', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[Cutoff].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.Cutoff', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[Cutoff].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Cutoff', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[Cutoff].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.Cutoff', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[Cutoff].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Cutoff', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[Cutoff].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.Department', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[Department].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.Department', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[Department].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.Department', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[Department].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Department', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[Department].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.Department', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[Department].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Department', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[Department].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.Position', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[Position].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.Position', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[Position].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.Position', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[Position].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Position', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[Position].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.Position', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[Position].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Position', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[Position].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.UserType', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[UserType].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.UserType', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[UserType].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.UserType', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[UserType].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.UserType', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[UserType].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.UserType', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[UserType].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.UserType', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[UserType].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.PermissionType', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionType].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionType', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionType].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionType', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionType].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionType', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionType].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionType', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionType].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionType', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionType].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.PermissionStatusType', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionStatusType].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionStatusType', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionStatusType].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionStatusType', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionStatusType].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionStatusType', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionStatusType].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionStatusType', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionStatusType].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.PermissionStatusType', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[PermissionStatusType].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.CutoffStatusType', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[CutoffStatusType].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.CutoffStatusType', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[CutoffStatusType].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.CutoffStatusType', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[CutoffStatusType].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.CutoffStatusType', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[CutoffStatusType].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.CutoffStatusType', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[CutoffStatusType].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.CutoffStatusType', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[CutoffStatusType].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.OriginType', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[OriginType].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.OriginType', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[OriginType].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.OriginType', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[OriginType].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.OriginType', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[OriginType].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.OriginType', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[OriginType].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.OriginType', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[OriginType].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.ApiKey', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKey].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKey', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKey].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKey', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKey].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKey', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKey].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKey', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKey].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.ApiFunction', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiFunction].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiFunction', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[ApiFunction].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiFunction', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiFunction].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiFunction', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[ApiFunction].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiFunction', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiFunction].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiFunction', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[ApiFunction].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.ApiPermission', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiPermission].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiPermission', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[ApiPermission].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiPermission', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiPermission].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiPermission', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[ApiPermission].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiPermission', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiPermission].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiPermission', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[ApiPermission].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.ApiKeyTablePermission', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKeyTablePermission].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKeyTablePermission', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKeyTablePermission].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKeyTablePermission', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKeyTablePermission].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKeyTablePermission', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKeyTablePermission].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKeyTablePermission', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKeyTablePermission].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.DatabaseTable', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[DatabaseTable].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.DatabaseTable', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[DatabaseTable].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.DatabaseTable', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[DatabaseTable].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.DatabaseTable', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[DatabaseTable].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.DatabaseTable', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[DatabaseTable].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.DatabaseTable', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[DatabaseTable].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.ResetPasswordToken', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[ResetPasswordToken].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.ResetPasswordToken', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[ResetPasswordToken].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ResetPasswordToken', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[ResetPasswordToken].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.ResetPasswordToken', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[ResetPasswordToken].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.ResetPasswordToken', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[ResetPasswordToken].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.AuditLog', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';

IF COL_LENGTH(N'dbo.Area', N'Descripcion') IS NOT NULL EXEC sp_rename N'[dbo].[Area].[Descripcion]', N'Description', N'COLUMN';
IF COL_LENGTH(N'dbo.Area', N'Estatus') IS NOT NULL EXEC sp_rename N'[dbo].[Area].[Estatus]', N'Status', N'COLUMN';
IF COL_LENGTH(N'dbo.Area', N'FechaCreacion') IS NOT NULL EXEC sp_rename N'[dbo].[Area].[FechaCreacion]', N'CreatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Area', N'IdUsuarioCrea') IS NOT NULL EXEC sp_rename N'[dbo].[Area].[IdUsuarioCrea]', N'CreatedByUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.Area', N'FechaModificacion') IS NOT NULL EXEC sp_rename N'[dbo].[Area].[FechaModificacion]', N'UpdatedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Area', N'IdUsuarioModifica') IS NOT NULL EXEC sp_rename N'[dbo].[Area].[IdUsuarioModifica]', N'UpdatedByUserId', N'COLUMN';
GO

-- =============================================================================
-- 3. RENAME TABLE-SPECIFIC COLUMNS
-- =============================================================================
-- Permission
IF COL_LENGTH(N'dbo.Permission', N'Motivo') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[Motivo]', N'Reason', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'Fecha1') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[Fecha1]', N'StartDate', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'Fecha2') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[Fecha2]', N'EndDate', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'Dias') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[Dias]', N'Days', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'Evidencia') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[Evidencia]', N'Evidence', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'Goce') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[Goce]', N'WithPay', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'FechaAutorizacion') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[FechaAutorizacion]', N'AuthorizedAt', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'IdEstadoPermiso') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[IdEstadoPermiso]', N'PermissionStatusTypeId', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'IdCorte') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[IdCorte]', N'CutoffId', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'IdTipoPermiso') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[IdTipoPermiso]', N'PermissionTypeId', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'IdUsuarioSolicita') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[IdUsuarioSolicita]', N'RequestingUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.Permission', N'IdUsuarioAutoriza') IS NOT NULL EXEC sp_rename N'[dbo].[Permission].[IdUsuarioAutoriza]', N'AuthorizingUserId', N'COLUMN';

-- [User]
IF COL_LENGTH(N'dbo.[User]', N'Nombre') IS NOT NULL EXEC sp_rename N'[dbo].[User].[Nombre]', N'Name', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'Correo') IS NOT NULL EXEC sp_rename N'[dbo].[User].[Correo]', N'Email', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'Clave') IS NOT NULL EXEC sp_rename N'[dbo].[User].[Clave]', N'PasswordHash', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'IdTipoUsuario') IS NOT NULL EXEC sp_rename N'[dbo].[User].[IdTipoUsuario]', N'UserTypeId', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'IdArea') IS NOT NULL EXEC sp_rename N'[dbo].[User].[IdArea]', N'AreaId', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'IdDepartamento') IS NOT NULL EXEC sp_rename N'[dbo].[User].[IdDepartamento]', N'DepartmentId', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'IdPuesto') IS NOT NULL EXEC sp_rename N'[dbo].[User].[IdPuesto]', N'PositionId', N'COLUMN';
IF COL_LENGTH(N'dbo.[User]', N'IdApiKey') IS NOT NULL EXEC sp_rename N'[dbo].[User].[IdApiKey]', N'ApiKeyId', N'COLUMN';

-- Cutoff
IF COL_LENGTH(N'dbo.Cutoff', N'Inicia') IS NOT NULL EXEC sp_rename N'[dbo].[Cutoff].[Inicia]', N'StartDate', N'COLUMN';
IF COL_LENGTH(N'dbo.Cutoff', N'Termina') IS NOT NULL EXEC sp_rename N'[dbo].[Cutoff].[Termina]', N'EndDate', N'COLUMN';
IF COL_LENGTH(N'dbo.Cutoff', N'IdEstadoCorte') IS NOT NULL EXEC sp_rename N'[dbo].[Cutoff].[IdEstadoCorte]', N'CutoffStatusTypeId', N'COLUMN';

-- Position
IF COL_LENGTH(N'dbo.Position', N'ClaveTabular') IS NOT NULL EXEC sp_rename N'[dbo].[Position].[ClaveTabular]', N'TabularCode', N'COLUMN';

-- ResetPasswordToken
IF COL_LENGTH(N'dbo.ResetPasswordToken', N'Utilizado') IS NOT NULL EXEC sp_rename N'[dbo].[ResetPasswordToken].[Utilizado]', N'Used', N'COLUMN';
IF COL_LENGTH(N'dbo.ResetPasswordToken', N'IdUsuarioSolicita') IS NOT NULL EXEC sp_rename N'[dbo].[ResetPasswordToken].[IdUsuarioSolicita]', N'RequestingUserId', N'COLUMN';
IF COL_LENGTH(N'dbo.ResetPasswordToken', N'FechaCaducidad') IS NOT NULL EXEC sp_rename N'[dbo].[ResetPasswordToken].[FechaCaducidad]', N'ExpiresAt', N'COLUMN';

-- ApiKeyTablePermission
IF COL_LENGTH(N'dbo.ApiKeyTablePermission', N'IdApiKey') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKeyTablePermission].[IdApiKey]', N'ApiKeyId', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKeyTablePermission', N'IdApiPermiso') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKeyTablePermission].[IdApiPermiso]', N'ApiPermissionId', N'COLUMN';
IF COL_LENGTH(N'dbo.ApiKeyTablePermission', N'IdTabla') IS NOT NULL EXEC sp_rename N'[dbo].[ApiKeyTablePermission].[IdTabla]', N'TableId', N'COLUMN';

-- AuditLog
IF COL_LENGTH(N'dbo.AuditLog', N'IdTabla') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[IdTabla]', N'TableId', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'IdRegistro') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[IdRegistro]', N'RecordId', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'IdApiFunction') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[IdApiFunction]', N'ApiFunctionId', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'IdApiKey') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[IdApiKey]', N'ApiKeyId', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'IdTipoOrigen') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[IdTipoOrigen]', N'OriginTypeId', N'COLUMN';
IF COL_LENGTH(N'dbo.AuditLog', N'DatosAnteriores') IS NOT NULL EXEC sp_rename N'[dbo].[AuditLog].[DatosAnteriores]', N'PreviousData', N'COLUMN';
GO

-- =============================================================================
-- 4. UPDATE DatabaseTable CATALOG (table names referenced by the audit machinery)
-- =============================================================================
UPDATE DatabaseTable SET Description = 'ApiFunction'      WHERE Description = 'ApiFunctions';
UPDATE DatabaseTable SET Description = 'ApiPermission'    WHERE Description = 'ApiPermiso';
UPDATE DatabaseTable SET Description = 'ApiKeyTablePermission' WHERE Description = 'ApiPermisosApiKeysTabla';
UPDATE DatabaseTable SET Description = 'Cutoff'            WHERE Description = 'Corte';
UPDATE DatabaseTable SET Description = 'Department'        WHERE Description = 'Departamento';
UPDATE DatabaseTable SET Description = 'Permission'        WHERE Description = 'Permiso';
UPDATE DatabaseTable SET Description = 'Position'          WHERE Description = 'Puesto';
UPDATE DatabaseTable SET Description = 'DatabaseTable'     WHERE Description = 'Tabla';
UPDATE DatabaseTable SET Description = 'PermissionType'    WHERE Description = 'TipoPermiso';
UPDATE DatabaseTable SET Description = 'UserType'          WHERE Description = 'TipoUsuario';
UPDATE DatabaseTable SET Description = 'User'              WHERE Description = 'Usuario';
UPDATE DatabaseTable SET Description = 'AuditLog'          WHERE Description = 'Auditoria';
UPDATE DatabaseTable SET Description = 'OriginType'        WHERE Description = 'TipoOrigen';
UPDATE DatabaseTable SET Description = 'CutoffStatusType'  WHERE Description = 'TipoEstadoCorte';
UPDATE DatabaseTable SET Description = 'PermissionStatusType' WHERE Description = 'TipoEstadoPermiso';
GO

-- =============================================================================
-- 5. DROP OLD STORED PROCEDURES AND TRIGGERS (recreated in English by schema_objects.sql)
-- =============================================================================
IF OBJECT_ID(N'dbo.LoginUsuario', N'P') IS NOT NULL DROP PROCEDURE [dbo].[LoginUsuario];
IF OBJECT_ID(N'dbo.BuscarPermisosFiltrados', N'P') IS NOT NULL DROP PROCEDURE [dbo].[BuscarPermisosFiltrados];
IF OBJECT_ID(N'dbo.UpdateFechaModificacion', N'P') IS NOT NULL DROP PROCEDURE [dbo].[UpdateFechaModificacion];
IF OBJECT_ID(N'dbo.trg_Permiso_Global', N'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_Permiso_Global];
IF OBJECT_ID(N'dbo.trg_Permiso_Auditoria', N'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_Permiso_Auditoria];
IF OBJECT_ID(N'dbo.trg_Permiso_UpdateFechaModificacion', N'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_Permiso_UpdateFechaModificacion];
IF OBJECT_ID(N'dbo.trg_TipoEstadoCorte_UpdateFechaModificacion', N'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_TipoEstadoCorte_UpdateFechaModificacion];
IF OBJECT_ID(N'dbo.trg_TipoEstadoPermiso_UpdateFechaModificacion', N'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_TipoEstadoPermiso_UpdateFechaModificacion];
IF OBJECT_ID(N'dbo.trg_Corte_Cascada_Cierre', N'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_Corte_Cascada_Cierre];
GO

PRINT 'Migration to English schema completed. Now run schema_objects.sql to recreate SPs and triggers.';
