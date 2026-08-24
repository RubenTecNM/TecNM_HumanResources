using ClosedXML.Excel;
using Xunit;

public class MiniXlTests
{
    [Fact]
    public void Dump()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Hoja");
        var h = ws.Cell(1, 1);
        h.Value = "ENCABEZADO";
        var e = ws.Cell(2, 1);
        e.Value = "ejemplo";
        ws.Cell(1, 2).Value = ("Otra");
        ws.Column(1).AdjustToContents();
        ws.SheetView.FreezeRows(1);
        
        using var ms = new System.IO.MemoryStream();
        wb.SaveAs(ms);
        System.IO.File.WriteAllBytes("/tmp/opencode/mini.xlsx", ms.ToArray());
    }
}
