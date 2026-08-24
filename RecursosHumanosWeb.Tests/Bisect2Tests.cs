using ClosedXML.Excel;
using Xunit;

public class Bisect2Tests
{
    private static void Save(XLWorkbook wb, string name)
    {
        using var ms = new System.IO.MemoryStream();
        wb.SaveAs(ms);
        System.IO.File.WriteAllBytes($"/tmp/opencode/{name}.xlsx", ms.ToArray());
    }

    // copia exacta de AddSheet
    private static void AddSheet(XLWorkbook wb, string sheetName, (string Header, string Example)[] columns)
    {
        var ws = wb.Worksheets.Add(sheetName);
        for (int i = 0; i < columns.Length; i++)
        {
            var h = ws.Cell(1, i + 1);
            h.Value = columns[i].Header;
            h.Style.Font.Bold = true;
            h.Style.Fill.BackgroundColor = XLColor.FromHtml("#1B396A");
            h.Style.Font.FontColor = XLColor.White;
            ws.Cell(2, i + 1).Value = columns[i].Example;
            ws.Column(i + 1).AdjustToContents();
        }
        ws.SheetView.FreezeRows(1);
    }

    [Fact]
    public void Dump()
    {
        // V4: AddSheet exacta, una hoja
        using (var wb = new XLWorkbook())
        {
            AddSheet(wb, "Autorizadores", new[]
            {
                ("Nombre", "María González Ruiz"),
                ("Correo", "mgonzalez@tecnm.mx")
            });
            Save(wb, "v4_addsheet");
        }
    }
}
