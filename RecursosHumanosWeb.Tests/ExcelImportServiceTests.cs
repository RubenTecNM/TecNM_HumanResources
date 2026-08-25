using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Services;
using Xunit;

namespace RecursosHumanosWeb.Tests;

public class ExcelImportServiceTests
{
    private static ExcelImportService CreateService(HumanResourcesContext context)
        => new(context);

    private static byte[] UsersWorkbook(params (string Name, string Email, string Tipo, string Area, string Depto, string Puesto)[] rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Usuarios");
        string[] headers = { "Nombre", "Correo", "Tipo", "Área", "Departamento", "Puesto" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        for (int r = 0; r < rows.Length; r++)
        {
            ws.Cell(r + 2, 1).Value = rows[r].Name;
            ws.Cell(r + 2, 2).Value = rows[r].Email;
            ws.Cell(r + 2, 3).Value = rows[r].Tipo;
            ws.Cell(r + 2, 4).Value = rows[r].Area;
            ws.Cell(r + 2, 5).Value = rows[r].Depto;
            ws.Cell(r + 2, 6).Value = rows[r].Puesto;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    [Fact]
    public async Task ValidateUsuarios_ExistenteYNuevo_ClassificaCorrectamente()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(ValidateUsuarios_ExistenteYNuevo_ClassificaCorrectamente));
        var service = CreateService(context);
        var bytes = UsersWorkbook(
            ("Empleado Test", "empleado@test.local", "Trabajador", "Área General", "Departamento General", "Puesto General"),
            ("Nuevo Docente", "nuevo@test.local", "", "Área General", "Departamento General", "Puesto General"));

        var preview = await service.ValidateAsync("usuarios", new MemoryStream(bytes));

        Assert.Equal(2, preview.TotalRows);
        Assert.Equal(1, preview.DuplicateCount);
        Assert.Equal(1, preview.NewCount);
        Assert.Empty(preview.Rows.Where(r => r.Status == "error"));
    }

    [Fact]
    public async Task ImportUsuarios_CreaNuevosConClaveTemporalYOmiteDuplicados()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(ImportUsuarios_CreaNuevosConClaveTemporalYOmiteDuplicados));
        var service = CreateService(context);
        var bytes = UsersWorkbook(
            ("Empleado Test", "empleado@test.local", "", "Área General", "Departamento General", "Puesto General"),
            ("Alfa Nuevo", "alfa@test.local", "Trabajador", "Área General", "Departamento General", "Puesto General"));

        var summary = await service.ImportAsync("usuarios", new MemoryStream(bytes), actorUserId: 1);

        Assert.Equal(1, summary.CreatedUsers);
        Assert.Equal(1, summary.Duplicates);

        var alfa = await context.Users.SingleAsync(u => u.Email == "alfa@test.local");
        Assert.Equal((int)UserTypeEnum.Employee, alfa.UserTypeId); // vacío = Trabajador
        Assert.False(string.IsNullOrEmpty(alfa.TempPassword), "el usuario nuevo debe tener clave temporal visible para RH");
        Assert.NotEqual(alfa.PasswordHash, alfa.TempPassword);
    }

    [Fact]
    public async Task ValidateUsuarios_CatalogoInexistente_ReportaErrorPorFila()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(ValidateUsuarios_CatalogoInexistente_ReportaErrorPorFila));
        var service = CreateService(context);
        var bytes = UsersWorkbook(("X", "x@test.local", "Trabajador", "ÁREA INEXISTENTE", "Departamento General", "Puesto General"));

        var preview = await service.ValidateAsync("usuarios", new MemoryStream(bytes));

        Assert.Equal(1, preview.ErrorCount);
        Assert.Contains("ÁREA INEXISTENTE", preview.Rows.Single(r => r.Status == "error").Message);
    }

    [Fact]
    public async Task ImportAutorizadores_PromueveExistenteYCreaNuevo()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(ImportAutorizadores_PromueveExistenteYCreaNuevo));
        var service = CreateService(context);

        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("Autorizadores");
            string[] headers = { "Nombre", "Correo", "Área", "Departamento", "Puesto" };
            for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(2, 1).Value = "Empleado Test";          // existe como Trabajador → promover
            ws.Cell(2, 2).Value = "empleado@test.local";
            ws.Cell(2, 3).Value = "Área General";
            ws.Cell(2, 4).Value = "Departamento General";
            ws.Cell(2, 5).Value = "Puesto General";
            ws.Cell(3, 1).Value = "Beta Autorizador";       // no existe → crear tipo Autorizador
            ws.Cell(3, 2).Value = "beta@test.local";
            ws.Cell(3, 3).Value = "Área General";
            ws.Cell(3, 4).Value = "Departamento General";
            ws.Cell(3, 5).Value = "Puesto General";
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var summary = await CreateService(context).ImportAsync("autorizadores", new MemoryStream(ms.ToArray()), 1);

            Assert.Equal(1, summary.PromotedAuthorizers);
            Assert.Equal(1, summary.CreatedUsers);
        }

        var promovido = await context.Users.SingleAsync(u => u.Email == "empleado@test.local");
        Assert.Equal((int)UserTypeEnum.Authorizer, promovido.UserTypeId);

        var beta = await context.Users.SingleAsync(u => u.Email == "beta@test.local");
        Assert.Equal((int)UserTypeEnum.Authorizer, beta.UserTypeId);
        Assert.False(string.IsNullOrEmpty(beta.TempPassword));
    }

    [Fact]
    public async Task ImportPermisos_CreaPeriodoCerradoYPermisosAprobadosSinGoce()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(ImportPermisos_CreaPeriodoCerradoYPermisosAprobadosSinGoce));
        var service = CreateService(context);

        // catálogo con Falta Docente para probar reglas por tipo
        context.PermissionTypes.Add(new PermissionType { Description = "Falta Docente", Status = true, CreatedByUserId = 1 });
        await context.SaveChangesAsync();

        using (var wb = new XLWorkbook())
        {
            var p = wb.Worksheets.Add("Periodos");
            p.Cell(1, 1).Value = "Fecha Inicio"; p.Cell(1, 2).Value = "Fecha Fin";
            p.Cell(2, 1).Value = "01/06/2026"; p.Cell(2, 2).Value = "30/06/2026";

            var m = wb.Worksheets.Add("Permisos");
            string[] h = { "Correo Solicitante", "Tipo Permiso", "Periodo Inicio", "Periodo Fin", "Fecha Inicio", "Fecha Fin", "Con Goce", "Motivo" };
            for (int i = 0; i < h.Length; i++) m.Cell(1, i + 1).Value = h[i];
            // fila válida: permiso general sin Con Goce → aprobado sin goce
            m.Cell(2, 1).Value = "empleado@test.local"; m.Cell(2, 2).Value = "Permiso General";
            m.Cell(2, 3).Value = "01/06/2026"; m.Cell(2, 4).Value = "30/06/2026";
            m.Cell(2, 5).Value = "10/06/2026"; m.Cell(2, 8).Value = "Asuntos personales";

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var summary = await CreateService(context).ImportAsync("permisos", new MemoryStream(ms.ToArray()), 1);

            Assert.Equal(1, summary.CreatedCutoffs);
            Assert.Equal(1, summary.CreatedPermissions);
        }

        var cutoff = await context.Cutoffs.SingleAsync(c => c.StartDate.Date == new DateTime(2026, 6, 1));
        Assert.Equal((int)CutoffStatusEnum.Closed, cutoff.CutoffStatusTypeId);

        var perm = await context.Permissions.Include(x => x.PermissionType)
            .SingleAsync(p => p.PermissionType.Description == "Permiso General" && p.RequestingUser.Email == "empleado@test.local");
        Assert.Equal((int)PermissionStatusEnum.Approved, perm.PermissionStatusTypeId);
        Assert.False(perm.WithPay);
        Assert.NotNull(perm.AuthorizedAt);
        Assert.Equal(cutoff.Id, perm.CutoffId);
    }

    [Fact]
    public async Task ImportPermisos_FaltaDocenteFueraDeDocencia_SeReportaComoError()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(ImportPermisos_FaltaDocenteFueraDeDocencia_SeReportaComoError));
        var service = CreateService(context);
        context.PermissionTypes.Add(new PermissionType { Description = "Falta Docente", Status = true, CreatedByUserId = 1 });
        await context.SaveChangesAsync();

        using var wb = new XLWorkbook();
        var p = wb.Worksheets.Add("Periodos");
        p.Cell(1, 1).Value = "Fecha Inicio"; p.Cell(1, 2).Value = "Fecha Fin";
        p.Cell(2, 1).Value = "01/07/2026"; p.Cell(2, 2).Value = "31/07/2026";

        var m = wb.Worksheets.Add("Permisos");
        string[] h = { "Correo Solicitante", "Tipo Permiso", "Periodo Inicio", "Periodo Fin", "Fecha Inicio", "Horas Aula", "Horas Descarga", "Motivo" };
        for (int i = 0; i < h.Length; i++) m.Cell(1, i + 1).Value = h[i];
        m.Cell(2, 1).Value = "empleado@test.local"; // NO es de DOCENCIA
        m.Cell(2, 2).Value = "Falta Docente";
        m.Cell(2, 3).Value = "01/07/2026"; m.Cell(2, 4).Value = "31/07/2026";
        m.Cell(2, 5).Value = "10/07/2026";
        m.Cell(2, 6).Value = "4"; m.Cell(2, 7).Value = "2";
        m.Cell(2, 8).Value = "prueba";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var summary = await service.ImportAsync("permisos", new MemoryStream(ms.ToArray()), 1);

        Assert.Equal(0, summary.CreatedPermissions);
        Assert.Contains(summary.Errors, e => e.Message.Contains("DOCENCIA"));
    }

    [Fact]
    public async Task ImportPermisos_ResuelvePeriodoPorIdDeHoja()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(ImportPermisos_ResuelvePeriodoPorIdDeHoja));

        using var wb = new XLWorkbook();
        var p = wb.Worksheets.Add("Periodos");
        p.Cell(1, 1).Value = "Id"; p.Cell(1, 2).Value = "Fecha Inicio"; p.Cell(1, 3).Value = "Fecha Fin";
        p.Cell(2, 1).Value = 1; p.Cell(2, 2).Value = "01/08/2026"; p.Cell(2, 3).Value = "10/09/2026";

        var m = wb.Worksheets.Add("Permisos");
        string[] h = { "Correo Solicitante", "Tipo Permiso", "Periodo (Id)", "Fecha Inicio", "Con Goce", "Motivo" };
        for (int i = 0; i < h.Length; i++) m.Cell(1, i + 1).Value = h[i];
        m.Cell(2, 1).Value = "empleado@test.local";
        m.Cell(2, 2).Value = "Permiso General";
        m.Cell(2, 3).Value = 1; // Id elegido del desplegable
        m.Cell(2, 4).Value = "20/08/2026";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var summary = await CreateService(context).ImportAsync("permisos", new MemoryStream(ms.ToArray()), 1);

        Assert.True(summary.Errors.Count == 0, string.Join(" | ", summary.Errors.Select(e => e.Message)));
        Assert.Equal(1, summary.CreatedCutoffs);
        Assert.Equal(1, summary.CreatedPermissions);
    }

    [Fact]
    public async Task ImportPermisos_IdInvalidoSeReportaComoError()
    {
        using var context = await TestDbContextFactory.SeedAsync(nameof(ImportPermisos_IdInvalidoSeReportaComoError));

        using var wb = new XLWorkbook();
        var p = wb.Worksheets.Add("Periodos");
        p.Cell(1, 1).Value = "Id"; p.Cell(1, 2).Value = "Fecha Inicio"; p.Cell(1, 3).Value = "Fecha Fin";
        p.Cell(2, 1).Value = 1; p.Cell(2, 2).Value = "01/08/2026"; p.Cell(2, 3).Value = "10/09/2026";

        var m = wb.Worksheets.Add("Permisos");
        string[] h = { "Correo Solicitante", "Tipo Permiso", "Periodo (Id)", "Fecha Inicio" };
        for (int i = 0; i < h.Length; i++) m.Cell(1, i + 1).Value = h[i];
        m.Cell(2, 1).Value = "empleado@test.local";
        m.Cell(2, 2).Value = "Permiso General";
        m.Cell(2, 3).Value = 9; // Id sin fechas en Periodos
        m.Cell(2, 4).Value = "20/08/2026";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var summary = await CreateService(context).ImportAsync("permisos", new MemoryStream(ms.ToArray()), 1);

        Assert.Equal(1, summary.Errors.Count);
        Assert.Equal(0, summary.CreatedPermissions);
    }
}
