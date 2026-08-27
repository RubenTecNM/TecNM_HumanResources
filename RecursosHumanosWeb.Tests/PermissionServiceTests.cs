using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Services;
using Xunit;

namespace RecursosHumanosWeb.Tests;

public class PermissionServiceTests
{
    // ── Tests base (8) ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePermissionAsync_ConAuthorizador_LanzaInvalidOperation()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreatePermissionAsync_ConAuthorizador_LanzaInvalidOperation));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var jefe = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Authorizer);
        var cutoff = await context.Cutoffs.SingleAsync();
        var permission = TestDbContextFactory.CreatePermission(jefe.Id, DateTime.Now, DateTime.Now, cutoff.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePermissionAsync(permission, jefe.Id));
    }

    [Fact]
    public async Task CreatePermissionAsync_DentroDelCutoff_QuedaPendiente()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreatePermissionAsync_DentroDelCutoff_QuedaPendiente));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();

        var permission = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id);

        var id = await service.CreatePermissionAsync(permission, empleado.Id);

        var creado = await context.Permissions.SingleAsync(p => p.Id == id);
        Assert.Equal(PermissionStatusEnum.Pending, (PermissionStatusEnum)creado.PermissionStatusTypeId);
        Assert.True(creado.WithPay);
        Assert.Equal(cutoff.Id, creado.CutoffId);
    }

    [Fact]
    public async Task CreatePermissionAsync_FueraDelCutoff_SeApruebaSinGoce()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreatePermissionAsync_FueraDelCutoff_SeApruebaSinGoce));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();

        var permission = TestDbContextFactory.CreatePermission(
            empleado.Id,
            cutoff.EndDate.AddDays(1),
            cutoff.EndDate.AddDays(2),
            cutoff.Id);

        var id = await service.CreatePermissionAsync(permission, empleado.Id);

        var creado = await context.Permissions.SingleAsync(p => p.Id == id);
        Assert.Equal(PermissionStatusEnum.Approved, (PermissionStatusEnum)creado.PermissionStatusTypeId);
        Assert.False(creado.WithPay);
        Assert.NotNull(creado.AuthorizedAt);
    }

    [Fact]
    public async Task CreatePermissionAsync_SinCutoffActivo_LanzaInvalidOperation()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreatePermissionAsync_SinCutoffActivo_LanzaInvalidOperation));
        context.Cutoffs.RemoveRange(context.Cutoffs);
        await context.SaveChangesAsync();

        var service = TestDbContextFactory.CreatePermissionService(context);
        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);

        var permission = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, 0);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePermissionAsync(permission, empleado.Id));
    }

    // ── Falta Docente (tipo 5) restringida a DOCENCIA ───────────────────────

    private static async Task<User> CrearDocenteAsync(HumanResourcesContext context, string nombre)
    {
        var docencia = context.Departments.Add(new Department
        { Description = "DOCENCIA", Status = true, CreatedByUserId = 1 }).Entity;
        await context.SaveChangesAsync();
        var docente = TestDbContextFactory.CreateUser(nombre, UserTypeEnum.Employee);
        docente.DepartmentId = docencia.Id;
        context.Users.Add(docente);
        await context.SaveChangesAsync();
        return docente;
    }

    private static Permission CreateTeacherAbsence(int requestingUserId, int cutoffId)
        => TestDbContextFactory.CreatePermission(requestingUserId, DateTime.Today.AddHours(9), null, cutoffId);

    [Fact]
    public async Task CreatePermissionAsync_FaltaDocente_DocentePuede()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreatePermissionAsync_FaltaDocente_DocentePuede));
        var service = TestDbContextFactory.CreatePermissionService(context);
        var docente = await CrearDocenteAsync(context, "docenteok");
        var cutoff = await context.Cutoffs.SingleAsync();

        var permission = CreateTeacherAbsence(docente.Id, cutoff.Id);
        permission.PermissionTypeId = 5;
        permission.HoursInClassroom = 4;
        permission.HoursDischarge = 2;

        var id = await service.CreatePermissionAsync(permission, docente.Id);

        var creado = await context.Permissions.SingleAsync(p => p.Id == id);
        Assert.Equal(PermissionStatusEnum.Pending, (PermissionStatusEnum)creado.PermissionStatusTypeId);
    }

    [Fact]
    public async Task CreatePermissionAsync_FaltaDocente_NoDocente_LanzaInvalidOperation()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreatePermissionAsync_FaltaDocente_NoDocente_LanzaInvalidOperation));
        var service = TestDbContextFactory.CreatePermissionService(context);
        var empleado = await context.Users.FirstAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();

        var permission = CreateTeacherAbsence(empleado.Id, cutoff.Id);
        permission.PermissionTypeId = 5;
        permission.HoursInClassroom = 4;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePermissionAsync(permission, empleado.Id));
        Assert.Contains("DOCENCIA", ex.Message);
    }

    [Fact]
    public async Task CreatePermissionAsync_FaltaDocente_RHParaDocente_Permite()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreatePermissionAsync_FaltaDocente_RHParaDocente_Permite));
        var service = TestDbContextFactory.CreatePermissionService(context);
        var docente = await CrearDocenteAsync(context, "docenterh");
        var rh = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.HumanResources);
        var cutoff = await context.Cutoffs.SingleAsync();

        var permission = CreateTeacherAbsence(docente.Id, cutoff.Id);
        permission.PermissionTypeId = 5;
        permission.HoursInClassroom = 3;
        permission.HoursDischarge = 1;

        var id = await service.CreatePermissionAsync(permission, rh.Id);
        Assert.True(id > 0);
    }

    [Fact]
    public async Task CreatePermissionAsync_FaltaDocente_RHParaNoDocente_LanzaInvalidOperation()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreatePermissionAsync_FaltaDocente_RHParaNoDocente_LanzaInvalidOperation));
        var service = TestDbContextFactory.CreatePermissionService(context);
        var empleado = await context.Users.FirstAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var rh = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.HumanResources);
        var cutoff = await context.Cutoffs.SingleAsync();

        var permission = CreateTeacherAbsence(empleado.Id, cutoff.Id);
        permission.PermissionTypeId = 5;
        permission.HoursInClassroom = 3;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePermissionAsync(permission, rh.Id));
    }

    [Fact]
    public async Task CancelPermissionAsync_CancelaPendienteEnCorteVigente()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CancelPermissionAsync_CancelaPendienteEnCorteVigente));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();
        var permission = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id);
        await context.Permissions.AddAsync(permission);
        await context.SaveChangesAsync();

        var ok = await service.CancelPermissionAsync(permission.Id, empleado.Id);

        Assert.True(ok);
        var actual = await context.Permissions.AsNoTracking().SingleAsync(p => p.Id == permission.Id);
        Assert.Equal(PermissionStatusEnum.Cancelled, (PermissionStatusEnum)actual.PermissionStatusTypeId);
        Assert.True(actual.Status);
    }

    [Fact]
    public async Task AuthorizePermissionAsync_RechazaYGuardaComentario()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(AuthorizePermissionAsync_RechazaYGuardaComentario));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var jefe = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Authorizer);
        var cutoff = await context.Cutoffs.SingleAsync();
        var permission = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, withPay: true);
        await context.Permissions.AddAsync(permission);
        await context.SaveChangesAsync();

        var ok = await service.AuthorizePermissionAsync(permission.Id, jefe.Id, aprobado: false, withPay: true, "Falta evidencia");

        Assert.True(ok);
        var actual = await context.Permissions.AsNoTracking().SingleAsync(p => p.Id == permission.Id);
        Assert.Equal(PermissionStatusEnum.Rejected, (PermissionStatusEnum)actual.PermissionStatusTypeId);
        Assert.False(actual.WithPay);
        Assert.Equal("Falta evidencia", actual.AuthorizationComment);
        Assert.Equal(jefe.Id, actual.AuthorizingUserId);
    }

    [Fact]
    public async Task GetStatsAsync_EmpleadoSoloVeSusPropiosPermisos()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(GetStatsAsync_EmpleadoSoloVeSusPropiosPermisos));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var empleado2 = await context.Users.SingleAsync(u => u.Name == "rh");
        var cutoff = await context.Cutoffs.SingleAsync();

        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Pending));
        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Approved));
        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(empleado2.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Approved));
        await context.SaveChangesAsync();

        var stats = await service.GetStatsAsync(empleado.Id, (int)UserTypeEnum.Employee);

        var type = stats.GetType();
        int Get(string prop) => (int)type.GetProperty(prop)!.GetValue(stats)!;

        Assert.Equal(2, Get("Total"));
        Assert.Equal(1, Get("Pending"));
        Assert.Equal(1, Get("Approved"));
    }

    [Fact]
    public async Task GetPermissionsPagedAsync_EmpleadoSoloVeSusPermisos()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(GetPermissionsPagedAsync_EmpleadoSoloVeSusPermisos));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var rh = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.HumanResources);
        var cutoff = await context.Cutoffs.SingleAsync();

        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id));
        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(rh.Id, DateTime.Now, DateTime.Now, cutoff.Id));
        await context.SaveChangesAsync();

        var page = await service.GetPermissionsPagedAsync(empleado.Id, null, "", null, null, null, 1, 10);

        Assert.Equal(1, page.TotalItemCount);
    }

    // ── Tests nuevos Fase 6 (+6) ─────────────────────────────────────────────

    [Fact]
    public async Task AuthorizePermissionAsync_ApruebaConGoce()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(AuthorizePermissionAsync_ApruebaConGoce));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var jefe = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Authorizer);
        var cutoff = await context.Cutoffs.SingleAsync();

        var perm = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id);
        await context.Permissions.AddAsync(perm);
        await context.SaveChangesAsync();

        var ok = await service.AuthorizePermissionAsync(perm.Id, jefe.Id, aprobado: true, withPay: true, null);

        Assert.True(ok);
        var actual = await context.Permissions.AsNoTracking().SingleAsync(p => p.Id == perm.Id);
        Assert.Equal(PermissionStatusEnum.Approved, (PermissionStatusEnum)actual.PermissionStatusTypeId);
        Assert.True(actual.WithPay);
        Assert.Equal(jefe.Id, actual.AuthorizingUserId);
    }

    [Fact]
    public async Task AuthorizePermissionAsync_ApruebaSinGoce()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(AuthorizePermissionAsync_ApruebaSinGoce));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var jefe = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Authorizer);
        var cutoff = await context.Cutoffs.SingleAsync();

        var perm = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, withPay: true);
        await context.Permissions.AddAsync(perm);
        await context.SaveChangesAsync();

        var ok = await service.AuthorizePermissionAsync(perm.Id, jefe.Id, aprobado: true, withPay: false, null);

        Assert.True(ok);
        var actual = await context.Permissions.AsNoTracking().SingleAsync(p => p.Id == perm.Id);
        Assert.Equal(PermissionStatusEnum.Approved, (PermissionStatusEnum)actual.PermissionStatusTypeId);
        // aprobado=true && withPay=false → WithPay debe ser false
        Assert.False(actual.WithPay);
    }

    [Fact]
    public async Task CanUserAccessPermiso_Propietario_AdminY_AutorizadorAreaTienenAcceso()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CanUserAccessPermiso_Propietario_AdminY_AutorizadorAreaTienenAcceso));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var admin = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Administrator);
        var jefe = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Authorizer);
        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();

        // Permiso pertenece al empleado, mismo área que el jefe (areaId=1 para todos)
        var perm = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id);
        await context.Permissions.AddAsync(perm);
        await context.SaveChangesAsync();

        // Cargar con navegación
        var permCargado = await context.Permissions
            .Include(p => p.RequestingUser)
            .SingleAsync(p => p.Id == perm.Id);

        // El propietario puede acceder
        Assert.True(service.CanUserAccessPermiso(empleado.Id, (int)UserTypeEnum.Employee, permCargado));

        // El admin puede acceder
        Assert.True(service.CanUserAccessPermiso(admin.Id, (int)UserTypeEnum.Administrator, permCargado));

        // El jefe del área puede acceder (mismo areaId)
        Assert.True(service.CanUserAccessPermiso(jefe.Id, (int)UserTypeEnum.Authorizer, permCargado));
    }

    [Fact]
    public async Task GetPermissionsPagedAsync_Autorizador_SoloVePropiosYPendientesDeArea()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(GetPermissionsPagedAsync_Autorizador_SoloVePropiosYPendientesDeArea));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var jefe = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Authorizer);
        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();

        // Permiso del empleado (misma área) — Pending → el autorizador debe verlo
        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Pending));
        // Permiso del propio jefe → siempre visible
        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(jefe.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Approved));
        await context.SaveChangesAsync();

        var page = await service.GetPermissionsPagedAsync(jefe.Id, null, "", null, null, null, 1, 10);

        Assert.Equal(2, page.TotalItemCount);
    }

    [Fact]
    public async Task GetPermissionsPagedAsync_Cancelado_VisibleSoloParaAdminYRH_NoAutorizador()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(GetPermissionsPagedAsync_Cancelado_VisibleSoloParaAdminYRH_NoAutorizador));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var admin = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Administrator);
        var rh = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.HumanResources);
        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var jefe = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Authorizer);
        var cutoff = await context.Cutoffs.SingleAsync();

        // Un permiso cancelado por su solicitante
        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Cancelled));
        await context.SaveChangesAsync();

        // Admin sí lo ve (tiene visibilidad total incluyendo cancelados)
        var pagAdmin = await service.GetPermissionsPagedAsync(admin.Id, null, "", null, null, (int)PermissionStatusEnum.Cancelled, 1, 10);
        Assert.Equal(1, pagAdmin.TotalItemCount);

        // RH también lo ve
        var pagRh = await service.GetPermissionsPagedAsync(rh.Id, null, "", null, null, (int)PermissionStatusEnum.Cancelled, 1, 10);
        Assert.Equal(1, pagRh.TotalItemCount);

        // El autorizador no ve permisos cancelados ajenos (solo lo propio o pendiente de su área)
        var pagAutorizador = await service.GetPermissionsPagedAsync(jefe.Id, null, "", null, null, (int)PermissionStatusEnum.Cancelled, 1, 10);
        Assert.Equal(0, pagAutorizador.TotalItemCount);
    }

    [Fact]
    public async Task UpdatePermissionAsync_SoloPendingYCorteAbierto()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(UpdatePermissionAsync_SoloPendingYCorteAbierto));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();

        // Permiso ya aprobado — no debe poder editarse
        var permAprobado = TestDbContextFactory.CreatePermission(
            empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Approved);
        await context.Permissions.AddAsync(permAprobado);
        await context.SaveChangesAsync();

        var permUpdate = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now.AddHours(1), DateTime.Now.AddHours(2), cutoff.Id);
        permUpdate.Id = permAprobado.Id;

        var result = await service.UpdatePermissionAsync(permUpdate, empleado.Id);

        Assert.False(result, "No se debe poder editar un permiso ya Approved");
    }
}
