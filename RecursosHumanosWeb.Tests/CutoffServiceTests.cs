using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Services;
using Xunit;

namespace RecursosHumanosWeb.Tests;

public class CutoffServiceTests
{
    // ── Tests base (4) ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveCutoffAsync_DevuelveElAbierto()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(GetActiveCutoffAsync_DevuelveElAbierto));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var activo = await service.GetActiveCutoffAsync();

        Assert.NotNull(activo);
        Assert.Equal(CutoffStatusEnum.Open, (CutoffStatusEnum)activo.CutoffStatusTypeId);
    }

    [Fact]
    public async Task HasPendingPermissionsAsync_DetectaPendientes()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(HasPendingPermissionsAsync_DetectaPendientes));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();

        Assert.False(await service.HasPendingPermissionsAsync(cutoff.Id));

        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Pending));
        await context.SaveChangesAsync();

        Assert.True(await service.HasPendingPermissionsAsync(cutoff.Id));
    }

    [Fact]
    public async Task AutoCloseCutoffIfNeededAsync_CierraVencidoYRechazaPendientes()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(AutoCloseCutoffIfNeededAsync_CierraVencidoYRechazaPendientes));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);

        // Corte activo con fecha de fin ya vencida
        var vencido = TestDbContextFactory.CreateCutoff(DateTime.Now.Date.AddMonths(-2), DateTime.Now.Date.AddDays(-10));
        context.Cutoffs.Add(vencido);
        await context.SaveChangesAsync();

        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now.AddMonths(-2), DateTime.Now.AddMonths(-2), vencido.Id, PermissionStatusEnum.Pending));
        await context.SaveChangesAsync();

        await service.AutoCloseCutoffIfNeededAsync();

        var cerrado = await context.Cutoffs.AsNoTracking().SingleAsync(c => c.Id == vencido.Id);
        Assert.Equal(CutoffStatusEnum.Closed, (CutoffStatusEnum)cerrado.CutoffStatusTypeId);

        var pendiente = await context.Permissions.AsNoTracking().SingleAsync(p => p.CutoffId == vencido.Id);
        Assert.Equal(PermissionStatusEnum.Rejected, (PermissionStatusEnum)pendiente.PermissionStatusTypeId);
        Assert.False(pendiente.WithPay);
    }

    [Fact]
    public async Task AutoCloseCutoffIfNeededAsync_NoTocaCutoffVigente()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(AutoCloseCutoffIfNeededAsync_NoTocaCutoffVigente));
        var service = TestDbContextFactory.CreateCutoffService(context);

        await service.AutoCloseCutoffIfNeededAsync();

        var activo = await context.Cutoffs.SingleAsync(c => c.CutoffStatusTypeId == (int)CutoffStatusEnum.Open);
        Assert.True(activo.EndDate >= DateTime.Now.Date);
    }

    [Fact]
    public async Task AutoCloseCutoffIfNeededAsync_InvalidaHuerfanosDeCorteYaCerrado()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(AutoCloseCutoffIfNeededAsync_InvalidaHuerfanosDeCorteYaCerrado));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);

        // Corte ya cerrado con un pendiente huérfano (dato externo al código)
        var cerrado = TestDbContextFactory.CreateCutoff(DateTime.Now.Date.AddMonths(-4), DateTime.Now.Date.AddMonths(-2), CutoffStatusEnum.Closed);
        context.Cutoffs.Add(cerrado);
        await context.SaveChangesAsync();

        var huerfano = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now.AddMonths(-3), DateTime.Now.AddMonths(-3), cerrado.Id, PermissionStatusEnum.Pending);
        context.Permissions.Add(huerfano);
        await context.SaveChangesAsync();

        await service.AutoCloseCutoffIfNeededAsync();

        var pendiente = await context.Permissions.AsNoTracking().SingleAsync(p => p.Id == huerfano.Id);
        // El barrido de huérfanos fue eliminado: un pendiente de un corte ya cerrado permanece Pending (histórico), sin alterar.
        Assert.Equal(PermissionStatusEnum.Pending, (PermissionStatusEnum)pendiente.PermissionStatusTypeId);
        Assert.True(pendiente.WithPay);
    }

    // ── Tests nuevos Fase 6 (+6) ─────────────────────────────────────────────

    [Fact]
    public async Task CreateCutoffAsync_TraslapeConActivo_NoCrea()
    {
        // Cubre las 3 condiciones de solapamiento:
        //   (a) inicio dentro del existente
        //   (b) término dentro del existente
        //   (c) abarca completamente al existente
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreateCutoffAsync_TraslapeConActivo_NoCrea));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var admin = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Administrator);
        var existing = await context.Cutoffs.SingleAsync();

        // (a) inicio dentro del periodo activo
        var overlapA = TestDbContextFactory.CreateCutoff(
            existing.StartDate.AddDays(5),
            existing.EndDate.AddDays(30));
        var resultA = await service.CreateCutoffAsync(overlapA, admin.Id);
        Assert.False(resultA, "Caso (a): inicio dentro del activo debe rechazarse");

        // (b) término dentro del periodo activo — necesita contexto limpio para no acumular el corte activo cerrado
        // Creamos fechas que caigan dentro del periodo existente desde antes
        var overlapB = TestDbContextFactory.CreateCutoff(
            existing.StartDate.AddDays(-10),
            existing.StartDate.AddDays(5));
        var resultB = await service.CreateCutoffAsync(overlapB, admin.Id);
        Assert.False(resultB, "Caso (b): término dentro del activo debe rechazarse");

        // (c) abarca completamente
        var overlapC = TestDbContextFactory.CreateCutoff(
            existing.StartDate.AddDays(-10),
            existing.EndDate.AddDays(10));
        var resultC = await service.CreateCutoffAsync(overlapC, admin.Id);
        Assert.False(resultC, "Caso (c): abarca completamente debe rechazarse");
    }

    [Fact]
    public async Task CreateCutoffAsync_SinPendientes_CierraActivoPrevio()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreateCutoffAsync_SinPendientes_CierraActivoPrevio));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var admin = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Administrator);
        var anterior = await context.Cutoffs.SingleAsync();

        // Nuevo corte sin solapamiento (empieza tras el fin del activo)
        var nuevo = TestDbContextFactory.CreateCutoff(
            anterior.EndDate.Date.AddDays(2),
            anterior.EndDate.Date.AddDays(16));

        var result = await service.CreateCutoffAsync(nuevo, admin.Id);

        Assert.True(result);

        var anteriorActualizado = await context.Cutoffs.AsNoTracking().SingleAsync(c => c.Id == anterior.Id);
        Assert.Equal(CutoffStatusEnum.Closed, (CutoffStatusEnum)anteriorActualizado.CutoffStatusTypeId);
    }

    [Fact]
    public async Task CreateCutoffAsync_ConPendientesEnActivo_NoPermite()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreateCutoffAsync_ConPendientesEnActivo_NoPermite));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var admin = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Administrator);
        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var actual = await context.Cutoffs.SingleAsync();

        // Insertar permiso pendiente en el corte activo
        await context.Permissions.AddAsync(TestDbContextFactory.CreatePermission(
            empleado.Id, DateTime.Now, DateTime.Now, actual.Id, PermissionStatusEnum.Pending));
        await context.SaveChangesAsync();

        var nuevo = TestDbContextFactory.CreateCutoff(
            actual.EndDate.Date.AddDays(2),
            actual.EndDate.Date.AddDays(16));

        var result = await service.CreateCutoffAsync(nuevo, admin.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task CloseCutoffAsync_RechazaPendientesSinGoce()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(CloseCutoffAsync_RechazaPendientesSinGoce));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var admin = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Administrator);
        var empleado = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Employee);
        var cutoff = await context.Cutoffs.SingleAsync();

        // Crear permiso pendiente con goce
        var perm = TestDbContextFactory.CreatePermission(empleado.Id, DateTime.Now, DateTime.Now, cutoff.Id, PermissionStatusEnum.Pending, withPay: true);
        await context.Permissions.AddAsync(perm);
        await context.SaveChangesAsync();

        var result = await service.CloseCutoffAsync(cutoff.Id, admin.Id);

        Assert.True(result);

        var cerrado = await context.Cutoffs.AsNoTracking().SingleAsync(c => c.Id == cutoff.Id);
        Assert.Equal(CutoffStatusEnum.Closed, (CutoffStatusEnum)cerrado.CutoffStatusTypeId);

        var permActualizado = await context.Permissions.AsNoTracking().SingleAsync(p => p.Id == perm.Id);
        Assert.Equal(PermissionStatusEnum.Rejected, (PermissionStatusEnum)permActualizado.PermissionStatusTypeId);
        Assert.False(permActualizado.WithPay, "Rechazo en cierre fuerza WithPay=false");
    }

    [Fact]
    public async Task CreateCutoffAsync_NormalizaEndDate()
    {
        // EndDate debe normalizarse a fecha.Date.AddDays(1).AddTicks(-1) (último tick del día)
        using var context = await TestDbContextFactory.SeedAsync(nameof(CreateCutoffAsync_NormalizaEndDate));
        var service = TestDbContextFactory.CreateCutoffService(context);

        var admin = await context.Users.SingleAsync(u => u.UserTypeId == (int)UserTypeEnum.Administrator);
        var anterior = await context.Cutoffs.SingleAsync();

        var startDate = anterior.EndDate.Date.AddDays(2);
        var endDate = anterior.EndDate.Date.AddDays(16); // fecha sin hora

        var nuevo = TestDbContextFactory.CreateCutoff(startDate, endDate);
        var result = await service.CreateCutoffAsync(nuevo, admin.Id);

        Assert.True(result);

        var guardado = await context.Cutoffs.AsNoTracking().SingleAsync(c => c.Id == nuevo.Id);
        var esperado = endDate.Date.AddDays(1).AddTicks(-1);
        Assert.Equal(esperado, guardado.EndDate);
    }
}
