-- =============================================================================
-- seed_catalogs.sql
-- Reconciliation (idempotent) of the reference/catalog tables.
-- Safe to run multiple times on an existing database:
--   * Updates Description/Color/Status of existing rows by Id.
--   * Inserts missing rows.
--   * Adds FK from the orphan column IdUsuarioModificaNavigationId -> User.Id
--     (leftover of the i18n migration; NOT dropped, per project decision).
-- Does NOT touch schema beyond the FKs described above.
--
-- NOTE: Apply ONLY to the TESTING database. Production is applied by the owner.
--   Usage: sqlcmd -S ... -d DB_Testing_HumanResources -i seed_catalogs.sql -f 65001
-- =============================================================================
SET NOCOUNT ON;

-- -----------------------------------------------------------------------------
-- 1. UserType
-- -----------------------------------------------------------------------------
UPDATE UserType SET Description = 'Administrador',  Status = 1 WHERE Id = 1;
UPDATE UserType SET Description = 'Autorizador',     Status = 1 WHERE Id = 2;
UPDATE UserType SET Description = 'Recursos Humanos', Status = 1 WHERE Id = 3;
UPDATE UserType SET Description = 'Trabajador',      Status = 1 WHERE Id = 4;

IF NOT EXISTS (SELECT 1 FROM UserType WHERE Id = 1) INSERT INTO UserType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (1, 'Administrador', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM UserType WHERE Id = 2) INSERT INTO UserType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (2, 'Autorizador', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM UserType WHERE Id = 3) INSERT INTO UserType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (3, 'Recursos Humanos', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM UserType WHERE Id = 4) INSERT INTO UserType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (4, 'Trabajador', GETDATE(), 1, 1);

-- -----------------------------------------------------------------------------
-- 2. PermissionType
-- -----------------------------------------------------------------------------
UPDATE PermissionType SET Description = 'Retardo',          Status = 1 WHERE Id = 1;
UPDATE PermissionType SET Description = 'Turno por Turno',  Status = 1 WHERE Id = 2;
UPDATE PermissionType SET Description = 'Falta',            Status = 1 WHERE Id = 3;
UPDATE PermissionType SET Description = 'Cambio de Horario', Status = 1 WHERE Id = 4;

IF NOT EXISTS (SELECT 1 FROM PermissionType WHERE Id = 1) INSERT INTO PermissionType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (1, 'Retardo', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionType WHERE Id = 2) INSERT INTO PermissionType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (2, 'Turno por Turno', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionType WHERE Id = 3) INSERT INTO PermissionType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (3, 'Falta', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionType WHERE Id = 4) INSERT INTO PermissionType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (4, 'Cambio de Horario', GETDATE(), 1, 1);

-- -----------------------------------------------------------------------------
-- 3. CutoffStatusType
-- -----------------------------------------------------------------------------
UPDATE CutoffStatusType SET Description = 'Abierto',   Color = 'success',   Status = 1 WHERE Id = 1;
UPDATE CutoffStatusType SET Description = 'Cerrado',   Color = 'secondary', Status = 1 WHERE Id = 2;
UPDATE CutoffStatusType SET Description = 'Invalidado', Color = 'danger',    Status = 1 WHERE Id = 3;

IF NOT EXISTS (SELECT 1 FROM CutoffStatusType WHERE Id = 1) INSERT INTO CutoffStatusType (Id, Description, Color, CreatedAt, Status, CreatedByUserId) VALUES (1, 'Abierto', 'success', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM CutoffStatusType WHERE Id = 2) INSERT INTO CutoffStatusType (Id, Description, Color, CreatedAt, Status, CreatedByUserId) VALUES (2, 'Cerrado', 'secondary', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM CutoffStatusType WHERE Id = 3) INSERT INTO CutoffStatusType (Id, Description, Color, CreatedAt, Status, CreatedByUserId) VALUES (3, 'Invalidado', 'danger', GETDATE(), 1, 1);

-- -----------------------------------------------------------------------------
-- 4. PermissionStatusType
-- -----------------------------------------------------------------------------
UPDATE PermissionStatusType SET Description = 'Pendiente por Revisar', Color = 'warning', Status = 1 WHERE Id = 1;
UPDATE PermissionStatusType SET Description = 'Revisado / Aprobado',    Color = 'success', Status = 1 WHERE Id = 2;
UPDATE PermissionStatusType SET Description = 'Invalidado por Periodo', Color = 'danger',  Status = 1 WHERE Id = 3;
UPDATE PermissionStatusType SET Description = 'Revisado / Rechazado',   Color = 'dark',    Status = 1 WHERE Id = 4;

IF NOT EXISTS (SELECT 1 FROM PermissionStatusType WHERE Id = 1) INSERT INTO PermissionStatusType (Id, Description, Color, CreatedAt, Status, CreatedByUserId) VALUES (1, 'Pendiente por Revisar', 'warning', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionStatusType WHERE Id = 2) INSERT INTO PermissionStatusType (Id, Description, Color, CreatedAt, Status, CreatedByUserId) VALUES (2, 'Revisado / Aprobado', 'success', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionStatusType WHERE Id = 3) INSERT INTO PermissionStatusType (Id, Description, Color, CreatedAt, Status, CreatedByUserId) VALUES (3, 'Invalidado por Periodo', 'danger', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionStatusType WHERE Id = 4) INSERT INTO PermissionStatusType (Id, Description, Color, CreatedAt, Status, CreatedByUserId) VALUES (4, 'Revisado / Rechazado', 'dark', GETDATE(), 1, 1);

-- -----------------------------------------------------------------------------
-- 5. OriginType
-- -----------------------------------------------------------------------------
UPDATE OriginType SET Description = 'Interfaz Web',      Status = 1 WHERE Id = 1;
UPDATE OriginType SET Description = 'API Externa',       Status = 1 WHERE Id = 2;
UPDATE OriginType SET Description = 'Sistema Automático', Status = 1 WHERE Id = 3;
UPDATE OriginType SET Description = 'Job Programado',    Status = 1 WHERE Id = 4;
UPDATE OriginType SET Description = 'Sincronización LDAP', Status = 1 WHERE Id = 5;
UPDATE OriginType SET Description = 'Migración de Datos', Status = 1 WHERE Id = 6;

IF NOT EXISTS (SELECT 1 FROM OriginType WHERE Id = 1) INSERT INTO OriginType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (1, 'Interfaz Web', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM OriginType WHERE Id = 2) INSERT INTO OriginType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (2, 'API Externa', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM OriginType WHERE Id = 3) INSERT INTO OriginType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (3, 'Sistema Automático', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM OriginType WHERE Id = 4) INSERT INTO OriginType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (4, 'Job Programado', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM OriginType WHERE Id = 5) INSERT INTO OriginType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (5, 'Sincronización LDAP', GETDATE(), 1, 1);
IF NOT EXISTS (SELECT 1 FROM OriginType WHERE Id = 6) INSERT INTO OriginType (Id, Description, CreatedAt, Status, CreatedByUserId) VALUES (6, 'Migración de Datos', GETDATE(), 1, 1);

-- -----------------------------------------------------------------------------
-- 6. FKs for orphan columns IdUsuarioModificaNavigationId -> User.Id
--    (These columns are leftovers of the i18n migration and are NOT dropped;
--     they are connected to the User table to indicate which user modified a row.)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PermissionStatusType_User_Modificador')
    ALTER TABLE [dbo].[PermissionStatusType] ADD CONSTRAINT [FK_PermissionStatusType_User_Modificador]
        FOREIGN KEY ([IdUsuarioModificaNavigationId]) REFERENCES [dbo].[User] ([Id]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CutoffStatusType_User_Modificador')
    ALTER TABLE [dbo].[CutoffStatusType] ADD CONSTRAINT [FK_CutoffStatusType_User_Modificador]
        FOREIGN KEY ([IdUsuarioModificaNavigationId]) REFERENCES [dbo].[User] ([Id]);

PRINT 'seed_catalogs.sql: OK';
