using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Services;
using Xunit;

namespace RecursosHumanosWeb.Tests;

public class PermissionServiceTests
{
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

    [Fact]
    public async Task DeletePermissionAsync_UsaBorradoLogico()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(DeletePermissionAsync_UsaBorradoLogico));
        var service = TestDbContextFactory.CreatePermissionService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();
        var permission = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id);
        await context.Permissions.AddAsync(permission);
        await context.SaveChangesAsync();

        var ok = await service.DeletePermissionAsync(permission.Id, empleado.Id);

        Assert.True(ok);
        var actual = await context.Permissions.AsNoTracking().SingleAsync(p => p.Id == permission.Id);
        Assert.Equal(PermissionStatusEnum.InvalidatedByPeriod, (PermissionStatusEnum)actual.PermissionStatusTypeId);
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
}
