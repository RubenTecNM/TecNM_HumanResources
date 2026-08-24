using RecursosHumanosWeb.Services;
using Xunit;

public class TplDumpTests
{
    [Fact]
    public void Dump()
    {
        System.IO.File.WriteAllBytes("/tmp/opencode/plantilla_autorizadores.xlsx", ExcelImportService.BuildTemplate("autorizadores"));
        System.IO.File.WriteAllBytes("/tmp/opencode/plantilla_usuarios.xlsx", ExcelImportService.BuildTemplate("usuarios"));
    }
}
