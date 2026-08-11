-- upgrade_003: Elimina el SP legacy SearchFilteredPermissions.
-- Motivo: no se usa en el código (.NET usa EF Core / X.PagedList) y su
-- definición referencia columnas antiguas (p.Status) que ya no existen
-- en el esquema actual (Permission.PermissionStatusTypeId).
-- Idempotente: solo elimina el procedimiento si existe.
IF OBJECT_ID(N'dbo.SearchFilteredPermissions', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE [dbo].[SearchFilteredPermissions];
END
