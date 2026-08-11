using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Services;
using Xunit;

namespace RecursosHumanosWeb.Tests;

public class CutoffServiceTests
{
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
}
