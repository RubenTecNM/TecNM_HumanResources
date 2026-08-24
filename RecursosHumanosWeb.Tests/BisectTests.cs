using ClosedXML.Excel;
using Xunit;

public class BisectTests
{
    private static void Save(XLWorkbook wb, string name)
    {
        using var ms = new System.IO.MemoryStream();
        wb.SaveAs(ms);
        System.IO.File.WriteAllBytes($"/tmp/opencode/{name}.xlsx", ms.ToArray());
    }

    [Fact]
    public void Dump()
    {
        // V1: dos hojas
        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("A");
            ws.Cell(1, 1).Value = "Col";
            ws.Cell(2, 1).Value = "ejemplo";
            var ws2 = wb.Worksheets.Add("B");
            ws2.Cell(1, 1).Value = "Ley";
            Save(wb, "v1_doshojas");
        }
        // V2: estilos de encabezado
        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("A");
            var h = ws.Cell(1, 1);
            h.Value = "Col";
            h.Style.Font.Bold = true;
            h.Style.Fill.BackgroundColor = XLColor.FromHtml("#1B396A");
            h.Style.Font.FontColor = XLColor.White;
            ws.Cell(2, 1).Value = "ejemplo";
            Save(wb, "v2_estilos");
        }
        // V3: AdjustToContents dentro del loop
        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("A");
            for (int i = 0; i < 3; i++)
            {
                ws.Cell(1, i + 1).Value = $"C{i}";
                ws.Cell(2, i + 1).Value = $"e{i}";
                ws.Column(i + 1).AdjustToContents();
            }
            ws.SheetView.FreezeRows(1);
            Save(wb, "v3_loop");
        }
    }
}
