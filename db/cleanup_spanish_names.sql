-- ============================================================
-- 0. Eliminar objetos muertos con columnas en espanol (sin uso por la app)
-- ============================================================
IF OBJECT_ID(N'dbo.SearchPermissionsDTO', N'U') IS NOT NULL DROP TABLE dbo.SearchPermissionsDTO;
IF OBJECT_ID(N'dbo.LoginResultDTO', N'U') IS NOT NULL DROP TABLE dbo.LoginResultDTO;
IF OBJECT_ID(N'dbo.VwPermissionsUsers', N'U') IS NOT NULL DROP TABLE dbo.VwPermissionsUsers;
IF OBJECT_ID(N'dbo.VwPermissionsUsers', N'V') IS NOT NULL DROP VIEW dbo.VwPermissionsUsers;
GO

-- ============================================================
-- Limpieza de nombres en espanol (constraints/indices) -> ingles
-- Aplica a: DB_Production_HumanResources (ejecutar manualmente por el responsable)
-- Conservados por decision del usuario: columna IdUsuarioModificaNavigationId y sus 4 FK/2 IX
-- ============================================================
SET NOCOUNT ON;
GO
-- FKs
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Area__IdUsuarioC__24285DB4' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Area__IdUsuarioC__24285DB4', N'FK_Area_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Area__IdUsuarioM__251C81ED' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Area__IdUsuarioM__251C81ED', N'FK_Area_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Corte__IdUsuario__74AE54BC' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Corte__IdUsuario__74AE54BC', N'FK_Cutoff_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Corte__IdUsuario__75A278F5' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Corte__IdUsuario__75A278F5', N'FK_Cutoff_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Departame__IdUsu__27F8EE98' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Departame__IdUsu__27F8EE98', N'FK_Department_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Departame__IdUsu__28ED12D1' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Departame__IdUsu__28ED12D1', N'FK_Department_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Permiso__IdCorte__2A6B46EF' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Permiso__IdCorte__2A6B46EF', N'FK_Permission_CutoffId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Permiso__IdTipoP__2B5F6B28' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Permiso__IdTipoP__2B5F6B28', N'FK_Permission_PermissionTypeId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Permiso__IdUsuar__2C538F61' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Permiso__IdUsuar__2C538F61', N'FK_Permission_RequestingUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Permiso__IdUsuar__2D47B39A' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Permiso__IdUsuar__2D47B39A', N'FK_Permission_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Permiso__IdUsuar__2E3BD7D3' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Permiso__IdUsuar__2E3BD7D3', N'FK_Permission_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Permiso__IdUsuar__2F2FFC0C' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Permiso__IdUsuar__2F2FFC0C', N'FK_Permission_AuthorizingUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Puesto__IdUsuari__308E3499' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Puesto__IdUsuari__308E3499', N'FK_Position_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Puesto__IdUsuari__318258D2' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Puesto__IdUsuari__318258D2', N'FK_Position_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__TipoPermi__IdUsu__345EC57D' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__TipoPermi__IdUsu__345EC57D', N'FK_PermissionType_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__TipoPermi__IdUsu__3552E9B6' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__TipoPermi__IdUsu__3552E9B6', N'FK_PermissionType_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__TipoUsuar__IdUsu__36470DEF' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__TipoUsuar__IdUsu__36470DEF', N'FK_UserType_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__TipoUsuar__IdUsu__373B3228' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__TipoUsuar__IdUsu__373B3228', N'FK_UserType_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Usuario__IdArea__382F5661' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Usuario__IdArea__382F5661', N'FK_User_AreaId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Usuario__IdDepar__39237A9A' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Usuario__IdDepar__39237A9A', N'FK_User_DepartmentId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Usuario__IdPuest__3A179ED3' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Usuario__IdPuest__3A179ED3', N'FK_User_PositionId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Usuario__IdTipoU__3B0BC30C' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Usuario__IdTipoU__3B0BC30C', N'FK_User_UserTypeId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Usuario__IdUsuar__3BFFE745' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Usuario__IdUsuar__3BFFE745', N'FK_User_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK__Usuario__IdUsuar__3CF40B7E' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK__Usuario__IdUsuar__3CF40B7E', N'FK_User_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiFunctions_Usuario_Crea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiFunctions_Usuario_Crea', N'FK_ApiFunction_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiFunctions_Usuario_Modifica' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiFunctions_Usuario_Modifica', N'FK_ApiFunction_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiKeys_Usuario_Crea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiKeys_Usuario_Crea', N'FK_ApiKey_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiKeys_Usuario_Modifica' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiKeys_Usuario_Modifica', N'FK_ApiKey_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiPermisos_Usuario_Crea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiPermisos_Usuario_Crea', N'FK_ApiPermission_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiPermisos_Usuario_Modifica' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiPermisos_Usuario_Modifica', N'FK_ApiPermission_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiPermisosApiKeysTablas_ApiKeys' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiPermisosApiKeysTablas_ApiKeys', N'FK_ApiKeyTablePermission_ApiKeyId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiPermisosApiKeysTablas_ApiPermisos' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiPermisosApiKeysTablas_ApiPermisos', N'FK_ApiKeyTablePermission_ApiPermissionId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiPermisosApiKeysTablas_Tablas' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiPermisosApiKeysTablas_Tablas', N'FK_ApiKeyTablePermission_TableId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiPermisosApiKeysTablas_UsuarioCrea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiPermisosApiKeysTablas_UsuarioCrea', N'FK_ApiKeyTablePermission_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ApiPermisosApiKeysTablas_UsuarioModifica' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ApiPermisosApiKeysTablas_UsuarioModifica', N'FK_ApiKeyTablePermission_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Auditoria_ApiFunction' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Auditoria_ApiFunction', N'FK_AuditLog_ApiFunctionId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Auditoria_ApiKey' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Auditoria_ApiKey', N'FK_AuditLog_ApiKeyId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Auditoria_Tabla' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Auditoria_Tabla', N'FK_AuditLog_TableId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Auditoria_TipoOrigen' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Auditoria_TipoOrigen', N'FK_AuditLog_OriginTypeId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Auditoria_UsuarioCrea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Auditoria_UsuarioCrea', N'FK_AuditLog_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Auditoria_UsuarioModifica' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Auditoria_UsuarioModifica', N'FK_AuditLog_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Corte_TipoEstadoCorte' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Corte_TipoEstadoCorte', N'FK_Cutoff_CutoffStatusTypeId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Permiso_TipoEstadoPermiso' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Permiso_TipoEstadoPermiso', N'FK_Permission_PermissionStatusTypeId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ResetPasswordToken_UsuarioCrea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ResetPasswordToken_UsuarioCrea', N'FK_ResetPasswordToken_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ResetPasswordToken_UsuarioModifica' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ResetPasswordToken_UsuarioModifica', N'FK_ResetPasswordToken_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_ResetPasswordToken_UsuarioSolicita' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_ResetPasswordToken_UsuarioSolicita', N'FK_ResetPasswordToken_RequestingUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Tablas_Usuario_Crea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Tablas_Usuario_Crea', N'FK_DatabaseTable_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_Tablas_Usuario_Modifica' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_Tablas_Usuario_Modifica', N'FK_DatabaseTable_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_TipoEstadoCorte_UsuarioCrea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_TipoEstadoCorte_UsuarioCrea', N'FK_CutoffStatusType_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_TipoEstadoPermiso_UsuarioCrea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_TipoEstadoPermiso_UsuarioCrea', N'FK_PermissionStatusType_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_TipoOrigen_Usuario_Crea' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_TipoOrigen_Usuario_Crea', N'FK_OriginType_CreatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_TipoOrigen_Usuario_Modifica' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'FK_TipoOrigen_Usuario_Modifica', N'FK_OriginType_UpdatedByUserId', N'OBJECT';
-- PKs
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__ApiFunct__3214EC0745513D7B' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__ApiFunct__3214EC0745513D7B', N'PK_ApiFunction', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__ApiKey__3214EC07C3DAD720' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__ApiKey__3214EC07C3DAD720', N'PK_ApiKey', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__ApiPermi__3214EC078F3D9269' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__ApiPermi__3214EC078F3D9269', N'PK_ApiKeyTablePermission', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__ApiPermi__3214EC07EB278C1E' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__ApiPermi__3214EC07EB278C1E', N'PK_ApiPermission', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__Area__3214EC07A750D956' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__Area__3214EC07A750D956', N'PK_Area', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__Corte__3214EC07702F01F4' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__Corte__3214EC07702F01F4', N'PK_Cutoff', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__Departam__3214EC071FFCF6A1' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__Departam__3214EC071FFCF6A1', N'PK_Department', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__Permiso__3214EC07503FFDCA' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__Permiso__3214EC07503FFDCA', N'PK_Permission', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__Puesto__3214EC07319CC249' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__Puesto__3214EC07319CC249', N'PK_Position', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__Tabla__3214EC0771DFB8B8' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__Tabla__3214EC0771DFB8B8', N'PK_DatabaseTable', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__TipoPerm__3214EC07AEB8B6FC' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__TipoPerm__3214EC07AEB8B6FC', N'PK_PermissionType', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__TipoUsua__3214EC070D31226E' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__TipoUsua__3214EC070D31226E', N'PK_UserType', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK__Usuario__3214EC07981AC028' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK__Usuario__3214EC07981AC028', N'PK_User', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK_Auditoria' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK_Auditoria', N'PK_AuditLog', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK_TipoEstadoCorte' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK_TipoEstadoCorte', N'PK_CutoffStatusType', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK_TipoEstadoPermiso' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK_TipoEstadoPermiso', N'PK_PermissionStatusType', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'PK_TipoOrigen' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'PK_TipoOrigen', N'PK_OriginType', N'OBJECT';
-- Defaults
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiFuncti__Estat__4D94879B' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiFuncti__Estat__4D94879B', N'DF_ApiFunction_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiFuncti__Fecha__4E88ABD4' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiFuncti__Fecha__4E88ABD4', N'DF_ApiFunction_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiFuncti__Fecha__4F7CD00D' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiFuncti__Fecha__4F7CD00D', N'DF_ApiFunction_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiFuncti__IdUsu__5070F446' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiFuncti__IdUsu__5070F446', N'DF_ApiFunction_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiKey__Estatus__534D60F1' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiKey__Estatus__534D60F1', N'DF_ApiKey_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiKey__FechaCre__5441852A' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiKey__FechaCre__5441852A', N'DF_ApiKey_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiKey__FechaMod__5535A963' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiKey__FechaMod__5535A963', N'DF_ApiKey_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiKey__IdUsuari__5629CD9C' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiKey__IdUsuari__5629CD9C', N'DF_ApiKey_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiPermis__Estat__59063A47' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiPermis__Estat__59063A47', N'DF_ApiPermission_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiPermis__Estat__5EBF139D' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiPermis__Estat__5EBF139D', N'DF_ApiKeyTablePermission_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiPermis__Fecha__59FA5E80' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiPermis__Fecha__59FA5E80', N'DF_ApiPermission_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiPermis__Fecha__5AEE82B9' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiPermis__Fecha__5AEE82B9', N'DF_ApiPermission_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiPermis__Fecha__5FB337D6' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiPermis__Fecha__5FB337D6', N'DF_ApiKeyTablePermission_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiPermis__Fecha__60A75C0F' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiPermis__Fecha__60A75C0F', N'DF_ApiKeyTablePermission_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiPermis__IdUsu__5BE2A6F2' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiPermis__IdUsu__5BE2A6F2', N'DF_ApiPermission_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ApiPermis__IdUsu__619B8048' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ApiPermis__IdUsu__619B8048', N'DF_ApiKeyTablePermission_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Area__Estatus__66603565' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Area__Estatus__66603565', N'DF_Area_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Area__FechaCreac__6754599E' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Area__FechaCreac__6754599E', N'DF_Area_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Area__FechaModif__68487DD7' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Area__FechaModif__68487DD7', N'DF_Area_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Area__IdUsuarioM__693CA210' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Area__IdUsuarioM__693CA210', N'DF_Area_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Auditoria__Estat__6C190EBB' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Auditoria__Estat__6C190EBB', N'DF_AuditLog_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Auditoria__Fecha__6D0D32F4' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Auditoria__Fecha__6D0D32F4', N'DF_AuditLog_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Corte__FechaCrea__72C60C4A' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Corte__FechaCrea__72C60C4A', N'DF_Cutoff_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Corte__FechaModi__73BA3083' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Corte__FechaModi__73BA3083', N'DF_Cutoff_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Corte__IdEstadoC__71D1E811' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Corte__IdEstadoC__71D1E811', N'DF_Cutoff_CutoffStatusTypeId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Corte__IdUsuario__74AE54BC' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Corte__IdUsuario__74AE54BC', N'DF_Cutoff_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Departame__Estat__778AC167' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Departame__Estat__778AC167', N'DF_Department_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Departame__Fecha__787EE5A0' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Departame__Fecha__787EE5A0', N'DF_Department_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Departame__Fecha__797309D9' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Departame__Fecha__797309D9', N'DF_Department_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Departame__IdUsu__7A672E12' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Departame__IdUsu__7A672E12', N'DF_Department_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Permiso__Estatus__7F2BE32F' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Permiso__Estatus__7F2BE32F', N'DF_Permission_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Permiso__FechaAu__7E37BEF6' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Permiso__FechaAu__7E37BEF6', N'DF_Permission_AuthorizedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Permiso__FechaCr__00200768' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Permiso__FechaCr__00200768', N'DF_Permission_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Permiso__FechaMo__01142BA1' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Permiso__FechaMo__01142BA1', N'DF_Permission_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Permiso__IdEstad__7D439ABD' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Permiso__IdEstad__7D439ABD', N'DF_Permission_PermissionStatusTypeId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Permiso__IdUsuar__02084FDA' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Permiso__IdUsuar__02084FDA', N'DF_Permission_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Puesto__Estatus__05D8E0BE' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Puesto__Estatus__05D8E0BE', N'DF_Position_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Puesto__FechaCre__06CD04F7' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Puesto__FechaCre__06CD04F7', N'DF_Position_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Puesto__FechaMod__07C12930' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Puesto__FechaMod__07C12930', N'DF_Position_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Puesto__IdUsuari__08B54D69' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Puesto__IdUsuari__08B54D69', N'DF_Position_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ResetPass__Estat__0C85DE4D' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ResetPass__Estat__0C85DE4D', N'DF_ResetPasswordToken_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ResetPass__Fecha__0D7A0286' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ResetPass__Fecha__0D7A0286', N'DF_ResetPasswordToken_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ResetPass__Fecha__0E6E26BF' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ResetPass__Fecha__0E6E26BF', N'DF_ResetPasswordToken_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ResetPass__IdUsu__0F624AF8' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ResetPass__IdUsu__0F624AF8', N'DF_ResetPasswordToken_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__ResetPass__Utili__0B91BA14' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__ResetPass__Utili__0B91BA14', N'DF_ResetPasswordToken_Used', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Tabla__Estatus__123EB7A3' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Tabla__Estatus__123EB7A3', N'DF_DatabaseTable_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Tabla__FechaCrea__1332DBDC' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Tabla__FechaCrea__1332DBDC', N'DF_DatabaseTable_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Tabla__FechaModi__14270015' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Tabla__FechaModi__14270015', N'DF_DatabaseTable_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Tabla__IdUsuario__151B244E' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Tabla__IdUsuario__151B244E', N'DF_DatabaseTable_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoEstad__Estat__17F790F9' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoEstad__Estat__17F790F9', N'DF_CutoffStatusType_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoEstad__Estat__1BC821DD' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoEstad__Estat__1BC821DD', N'DF_PermissionStatusType_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoEstad__Fecha__18EBB532' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoEstad__Fecha__18EBB532', N'DF_CutoffStatusType_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoEstad__Fecha__1CBC4616' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoEstad__Fecha__1CBC4616', N'DF_PermissionStatusType_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoOrige__Estat__1F98B2C1' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoOrige__Estat__1F98B2C1', N'DF_OriginType_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoOrige__Fecha__208CD6FA' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoOrige__Fecha__208CD6FA', N'DF_OriginType_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoPermi__Estat__236943A5' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoPermi__Estat__236943A5', N'DF_PermissionType_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoPermi__Fecha__245D67DE' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoPermi__Fecha__245D67DE', N'DF_PermissionType_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoPermi__Fecha__25518C17' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoPermi__Fecha__25518C17', N'DF_PermissionType_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoPermi__IdUsu__2645B050' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoPermi__IdUsu__2645B050', N'DF_PermissionType_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoUsuar__Estat__29221CFB' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoUsuar__Estat__29221CFB', N'DF_UserType_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoUsuar__Fecha__2A164134' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoUsuar__Fecha__2A164134', N'DF_UserType_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoUsuar__Fecha__2B0A656D' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoUsuar__Fecha__2B0A656D', N'DF_UserType_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__TipoUsuar__IdUsu__2BFE89A6' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__TipoUsuar__IdUsu__2BFE89A6', N'DF_UserType_UpdatedByUserId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Usuario__Estatus__2FCF1A8A' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Usuario__Estatus__2FCF1A8A', N'DF_User_Status', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Usuario__FechaCr__30C33EC3' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Usuario__FechaCr__30C33EC3', N'DF_User_CreatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Usuario__FechaMo__31B762FC' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Usuario__FechaMo__31B762FC', N'DF_User_UpdatedAt', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Usuario__IdApiKe__2EDAF651' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Usuario__IdApiKe__2EDAF651', N'DF_User_ApiKeyId', N'OBJECT';
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF__Usuario__IdUsuar__32AB8735' AND type IN ('F','PK','UQ','D'))
    EXEC sp_rename N'DF__Usuario__IdUsuar__32AB8735', N'DF_User_UpdatedByUserId', N'OBJECT';
-- Indices (nombre calificado)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiFunction_IdUsuarioCrea')
    EXEC sp_rename N'dbo.ApiFunction.IX_ApiFunction_IdUsuarioCrea', N'IX_ApiFunction_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiFunction_IdUsuarioModifica')
    EXEC sp_rename N'dbo.ApiFunction.IX_ApiFunction_IdUsuarioModifica', N'IX_ApiFunction_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiKey_IdUsuarioCrea')
    EXEC sp_rename N'dbo.ApiKey.IX_ApiKey_IdUsuarioCrea', N'IX_ApiKey_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiKey_IdUsuarioModifica')
    EXEC sp_rename N'dbo.ApiKey.IX_ApiKey_IdUsuarioModifica', N'IX_ApiKey_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiPermiso_IdUsuarioCrea')
    EXEC sp_rename N'dbo.ApiPermission.IX_ApiPermiso_IdUsuarioCrea', N'IX_ApiPermission_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiPermiso_IdUsuarioModifica')
    EXEC sp_rename N'dbo.ApiPermission.IX_ApiPermiso_IdUsuarioModifica', N'IX_ApiPermission_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiPermisosApiKeysTabla_IdApiKey')
    EXEC sp_rename N'dbo.ApiKeyTablePermission.IX_ApiPermisosApiKeysTabla_IdApiKey', N'IX_ApiKeyTablePermission_ApiKeyId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiPermisosApiKeysTabla_IdApiPermiso')
    EXEC sp_rename N'dbo.ApiKeyTablePermission.IX_ApiPermisosApiKeysTabla_IdApiPermiso', N'IX_ApiKeyTablePermission_ApiPermissionId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiPermisosApiKeysTabla_IdTabla')
    EXEC sp_rename N'dbo.ApiKeyTablePermission.IX_ApiPermisosApiKeysTabla_IdTabla', N'IX_ApiKeyTablePermission_TableId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiPermisosApiKeysTabla_IdUsuarioCrea')
    EXEC sp_rename N'dbo.ApiKeyTablePermission.IX_ApiPermisosApiKeysTabla_IdUsuarioCrea', N'IX_ApiKeyTablePermission_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiPermisosApiKeysTabla_IdUsuarioModifica')
    EXEC sp_rename N'dbo.ApiKeyTablePermission.IX_ApiPermisosApiKeysTabla_IdUsuarioModifica', N'IX_ApiKeyTablePermission_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Area_IdUsuarioCrea')
    EXEC sp_rename N'dbo.Area.IX_Area_IdUsuarioCrea', N'IX_Area_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Area_IdUsuarioModifica')
    EXEC sp_rename N'dbo.Area.IX_Area_IdUsuarioModifica', N'IX_Area_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Auditoria_IdApiFunction')
    EXEC sp_rename N'dbo.AuditLog.IX_Auditoria_IdApiFunction', N'IX_AuditLog_ApiFunctionId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Auditoria_IdApiKey')
    EXEC sp_rename N'dbo.AuditLog.IX_Auditoria_IdApiKey', N'IX_AuditLog_ApiKeyId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Auditoria_IdTabla')
    EXEC sp_rename N'dbo.AuditLog.IX_Auditoria_IdTabla', N'IX_AuditLog_TableId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Auditoria_IdTipoOrigen')
    EXEC sp_rename N'dbo.AuditLog.IX_Auditoria_IdTipoOrigen', N'IX_AuditLog_OriginTypeId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Auditoria_IdUsuarioCrea')
    EXEC sp_rename N'dbo.AuditLog.IX_Auditoria_IdUsuarioCrea', N'IX_AuditLog_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Auditoria_IdUsuarioModifica')
    EXEC sp_rename N'dbo.AuditLog.IX_Auditoria_IdUsuarioModifica', N'IX_AuditLog_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Corte_IdEstadoCorte')
    EXEC sp_rename N'dbo.Cutoff.IX_Corte_IdEstadoCorte', N'IX_Cutoff_CutoffStatusTypeId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Corte_IdUsuarioCrea')
    EXEC sp_rename N'dbo.Cutoff.IX_Corte_IdUsuarioCrea', N'IX_Cutoff_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Corte_IdUsuarioModifica')
    EXEC sp_rename N'dbo.Cutoff.IX_Corte_IdUsuarioModifica', N'IX_Cutoff_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Departamento_IdUsuarioCrea')
    EXEC sp_rename N'dbo.Department.IX_Departamento_IdUsuarioCrea', N'IX_Department_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Departamento_IdUsuarioModifica')
    EXEC sp_rename N'dbo.Department.IX_Departamento_IdUsuarioModifica', N'IX_Department_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Permiso_IdCorte')
    EXEC sp_rename N'dbo.Permission.IX_Permiso_IdCorte', N'IX_Permission_CutoffId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Permiso_IdEstadoPermiso')
    EXEC sp_rename N'dbo.Permission.IX_Permiso_IdEstadoPermiso', N'IX_Permission_PermissionStatusTypeId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Permiso_IdTipoPermiso')
    EXEC sp_rename N'dbo.Permission.IX_Permiso_IdTipoPermiso', N'IX_Permission_PermissionTypeId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Permiso_IdUsuarioAutoriza')
    EXEC sp_rename N'dbo.Permission.IX_Permiso_IdUsuarioAutoriza', N'IX_Permission_AuthorizingUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Permiso_IdUsuarioCrea')
    EXEC sp_rename N'dbo.Permission.IX_Permiso_IdUsuarioCrea', N'IX_Permission_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Permiso_IdUsuarioModifica')
    EXEC sp_rename N'dbo.Permission.IX_Permiso_IdUsuarioModifica', N'IX_Permission_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Permiso_IdUsuarioSolicita')
    EXEC sp_rename N'dbo.Permission.IX_Permiso_IdUsuarioSolicita', N'IX_Permission_RequestingUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Puesto_IdUsuarioCrea')
    EXEC sp_rename N'dbo.Position.IX_Puesto_IdUsuarioCrea', N'IX_Position_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Puesto_IdUsuarioModifica')
    EXEC sp_rename N'dbo.Position.IX_Puesto_IdUsuarioModifica', N'IX_Position_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ResetPasswordToken_IdUsuarioCrea')
    EXEC sp_rename N'dbo.ResetPasswordToken.IX_ResetPasswordToken_IdUsuarioCrea', N'IX_ResetPasswordToken_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ResetPasswordToken_IdUsuarioModifica')
    EXEC sp_rename N'dbo.ResetPasswordToken.IX_ResetPasswordToken_IdUsuarioModifica', N'IX_ResetPasswordToken_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ResetPasswordToken_IdUsuarioSolicita')
    EXEC sp_rename N'dbo.ResetPasswordToken.IX_ResetPasswordToken_IdUsuarioSolicita', N'IX_ResetPasswordToken_RequestingUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tabla_IdUsuarioCrea')
    EXEC sp_rename N'dbo.DatabaseTable.IX_Tabla_IdUsuarioCrea', N'IX_DatabaseTable_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tabla_IdUsuarioModifica')
    EXEC sp_rename N'dbo.DatabaseTable.IX_Tabla_IdUsuarioModifica', N'IX_DatabaseTable_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoEstadoCorte_IdUsuarioCrea')
    EXEC sp_rename N'dbo.CutoffStatusType.IX_TipoEstadoCorte_IdUsuarioCrea', N'IX_CutoffStatusType_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoEstadoPermiso_IdUsuarioCrea')
    EXEC sp_rename N'dbo.PermissionStatusType.IX_TipoEstadoPermiso_IdUsuarioCrea', N'IX_PermissionStatusType_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoOrigen_IdUsuarioCrea')
    EXEC sp_rename N'dbo.OriginType.IX_TipoOrigen_IdUsuarioCrea', N'IX_OriginType_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoOrigen_IdUsuarioModifica')
    EXEC sp_rename N'dbo.OriginType.IX_TipoOrigen_IdUsuarioModifica', N'IX_OriginType_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoPermiso_IdUsuarioCrea')
    EXEC sp_rename N'dbo.PermissionType.IX_TipoPermiso_IdUsuarioCrea', N'IX_PermissionType_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoPermiso_IdUsuarioModifica')
    EXEC sp_rename N'dbo.PermissionType.IX_TipoPermiso_IdUsuarioModifica', N'IX_PermissionType_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoUsuario_IdUsuarioCrea')
    EXEC sp_rename N'dbo.UserType.IX_TipoUsuario_IdUsuarioCrea', N'IX_UserType_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TipoUsuario_IdUsuarioModifica')
    EXEC sp_rename N'dbo.UserType.IX_TipoUsuario_IdUsuarioModifica', N'IX_UserType_UpdatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuario_IdArea')
    EXEC sp_rename N'dbo.User.IX_Usuario_IdArea', N'IX_User_AreaId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuario_IdDepartamento')
    EXEC sp_rename N'dbo.User.IX_Usuario_IdDepartamento', N'IX_User_DepartmentId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuario_IdPuesto')
    EXEC sp_rename N'dbo.User.IX_Usuario_IdPuesto', N'IX_User_PositionId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuario_IdTipoUsuario')
    EXEC sp_rename N'dbo.User.IX_Usuario_IdTipoUsuario', N'IX_User_UserTypeId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuario_IdUsuarioCrea')
    EXEC sp_rename N'dbo.User.IX_Usuario_IdUsuarioCrea', N'IX_User_CreatedByUserId', N'INDEX';
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Usuario_IdUsuarioModifica')
    EXEC sp_rename N'dbo.User.IX_Usuario_IdUsuarioModifica', N'IX_User_UpdatedByUserId', N'INDEX';
GO
