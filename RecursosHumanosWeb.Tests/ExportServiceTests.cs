using System.Text;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Services;

namespace RecursosHumanosWeb.Tests;

public class ExportServiceTests
{
    private static async Task<HumanResourcesContext> PrepararAsync(string dbName)
    {
        var context = await TestDbContextFactory.SeedAsync(dbName);

        context.PermissionStatusTypes.Add(new PermissionStatusType { Id = 2, Description = "Revisado / Aprobado", Status = true });
        context.PermissionStatusTypes.Add(new PermissionStatusType { Id = 4, Description = "Revisado / Rechazado", Status = true });

        // Segundo jefe del área general para probar nombres unidos
        var jefe2 = TestDbContextFactory.CreateUser("jefe2", UserTypeEnum.Authorizer);
        context.Users.Add(jefe2);

        // Área sin jefes
        var areaSinJefe = context.Areas.Add(new Area { Description = "Área Sin Jefe", Status = true, CreatedByUserId = 1 }).Entity;
        await context.SaveChangesAsync();

        var solitario = TestDbContextFactory.CreateUser("solitario", UserTypeEnum.Employee);
        solitario.AreaId = areaSinJefe.Id;
        context.Users.Add(solitario);
        await context.SaveChangesAsync();

        var cutoff = await context.Cutoffs.SingleAsync();

        context.Permissions.Add(TestDbContextFactory.CreatePermission(
            context.Users.Single(u => u.Email == "empleado@test.local").Id,
            DateTime.Today, DateTime.Today.AddDays(2), cutoff.Id,
            PermissionStatusEnum.Approved));
        context.Permissions.Add(TestDbContextFactory.CreatePermission(
            solitario.Id, DateTime.Today, null, cutoff.Id, PermissionStatusEnum.Rejected));

        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task GenerateCutoffCsvAsync_UsaEspanolYResuelveCampos()
    {
        using var context = await PrepararAsync(nameof(GenerateCutoffCsvAsync_UsaEspanolYResuelveCampos));
        var service = new ExportService(context);
        var cutoff = await context.Cutoffs.SingleAsync();

        var (content, fileName) = await service.GenerateCutoffCsvAsync(cutoff.Id);

        Assert.NotNull(content);
        Assert.Contains(".csv", fileName);
        var csv = Encoding.UTF8.GetString(content);

        // Encabezados en español (adiós "Tipo Permission", "Reason", "Con WithPay")
        Assert.Contains("Tipo de Permiso", csv);
        Assert.Contains("Motivo", csv);
        Assert.Contains("Con Goce", csv);
        Assert.Contains("Estado", csv);

        // Días calculados por fechas aunque Days venga NULL en BD
        Assert.Contains(",3,", csv);   // rango de 3 días

        // Evidencia como bandera Sí/No
        Assert.Contains(",No,", csv);

        // Estado desde el catálogo
        Assert.Contains("Revisado / Aprobado", csv);

        // Jefe de área: dos nombres juntos cuando hay dos autorizadores
        Assert.Contains("jefe, jefe2", csv);

        // Área sin autorizadores -> Por definir
        Assert.Contains("Por definir", csv);

        // Status siempre activo en filas exportadas
        Assert.Contains(",1,", csv);
    }
}
