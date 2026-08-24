using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.Enums;
using System.Text;
using System.Xml;

namespace RecursosHumanosWeb.Services
{
    public class ExportService : IExportService
    {
        private readonly HumanResourcesContext _context;

        public ExportService(HumanResourcesContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fila de exportación ya resuelta: estado desde catálogo, días calculados,
        /// jefes de área y bandera de evidencia listos para serializar en cualquier formato.
        /// </summary>
        private sealed record ExportRow(
            int IdSolicitud,
            string Solicitante,
            string Area,
            string TipoPermiso,
            string Motivo,
            int DiasSolicitados,
            string ConGoce,
            string EvidenciaAdjunta,
            string Estado,
            int Status,
            string AutorizadoPor,
            DateTime? DesdeElDia,
            DateTime? HastaElDia,
            DateTime FechaDeSolicitud,
            DateTime? FechaDeAutorizacion,
            int IdSolicitante,
            string CorreoDelSolicitante);

        // Días: fin nulo → 1 día; si no, rango inclusive. Fallback al valor capturado.
        private static int CalcularDias(DateTime? inicio, DateTime? fin, int? dias)
            => inicio.HasValue && fin.HasValue ? (int)(fin.Value.Date - inicio.Value.Date).TotalDays + 1
             : inicio.HasValue ? 1
             : dias ?? 0;

        private async Task<List<ExportRow>> BuildRowsAsync(int cutoffId)
        {
            var estados = await _context.PermissionStatusTypes.AsNoTracking()
                .ToDictionaryAsync(s => s.Id, s => s.Description);

            // Jefe(s) de área = usuarios Autorizador activos del área del solicitante.
            var jefes = await _context.Users.AsNoTracking()
                .Where(u => u.UserTypeId == (int)UserTypeEnum.Authorizer && u.Status)
                .Select(u => new { u.AreaId, u.Name })
                .ToListAsync();

            var jefesPorArea = jefes
                .GroupBy(u => u.AreaId)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(u => u.Name)));

            var permisos = await _context.Permissions.AsNoTracking()
                .Where(p => p.CutoffId == cutoffId &&
                            p.PermissionStatusTypeId != (int)PermissionStatusEnum.InvalidatedByPeriod)
                .Include(p => p.RequestingUser).ThenInclude(u => u!.Area)
                .Include(p => p.PermissionType)
                .ToListAsync();

            return permisos.Select(p => new ExportRow(
                p.Id,
                p.RequestingUser?.Name ?? "",
                p.RequestingUser?.Area?.Description ?? "",
                p.PermissionType?.Description ?? "",
                p.Reason ?? "",
                CalcularDias(p.StartDate, p.EndDate, p.Days),
                p.WithPay ? "Sí" : "No",
                string.IsNullOrWhiteSpace(p.Evidence) ? "No" : "Sí",
                estados.GetValueOrDefault(p.PermissionStatusTypeId, ""),
                p.Status ? 1 : 0,
                jefesPorArea.TryGetValue(p.RequestingUser?.AreaId ?? 0, out var jefe) ? jefe : "Por definir",
                p.StartDate,
                p.EndDate,
                p.CreatedAt,
                p.AuthorizedAt,
                p.RequestingUserId,
                p.RequestingUser?.Email ?? ""
            )).OrderBy(r => r.Solicitante).ThenBy(r => r.IdSolicitud).ToList();
        }

        public async Task<byte[]> GenerateCutoffExcelAsync(int cutoffId)
        {
            var cutoff = await _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == cutoffId);

            if (cutoff == null) return null!;

            var filas = await BuildRowsAsync(cutoffId);
            var areas = await _context.Areas.OrderBy(a => a.Description).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                foreach (var area in areas)
                {
                    var delArea = filas.Where(f => f.Area == area.Description).ToList();
                    if (!delArea.Any()) continue;

                    var hoja = workbook.Worksheets.Add($"Área - {area.Id}");
                    var headers = new[]
                    {
                        "ID_SOLICITUD", "NOMBRE_DEL_SOLICITANTE", "ÁREA", "TIPO_DE_PERMISO",
                        "MOTIVO", "DÍAS_SOLICITADOS", "CON_GOCE", "EVIDENCIA_ADJUNTA", "ESTADO", "STATUS",
                        "AUTORIZADO_POR", "DESDE_EL_DÍA", "HASTA_EL_DÍA",
                        "FECHA_DE_SOLICITUD", "FECHA_DE_AUTORIZACIÓN",
                        "ID_SOLICITANTE", "CORREO_DEL_SOLICITANTE"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        hoja.Cell(1, i + 1).Value = headers[i];
                        hoja.Cell(1, i + 1).Style.Font.Bold = true;
                        hoja.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    }

                    int row = 2;
                    foreach (var f in delArea)
                    {
                        hoja.Cell(row, 1).Value = f.IdSolicitud;
                        hoja.Cell(row, 2).Value = f.Solicitante;
                        hoja.Cell(row, 3).Value = f.Area;
                        hoja.Cell(row, 4).Value = f.TipoPermiso;
                        hoja.Cell(row, 5).Value = f.Motivo;
                        hoja.Cell(row, 6).Value = f.DiasSolicitados;
                        hoja.Cell(row, 7).Value = f.ConGoce;
                        hoja.Cell(row, 8).Value = f.EvidenciaAdjunta;
                        hoja.Cell(row, 9).Value = f.Estado;
                        hoja.Cell(row, 10).Value = f.Status;
                        hoja.Cell(row, 11).Value = f.AutorizadoPor;
                        hoja.Cell(row, 12).Value = f.DesdeElDia?.ToString("yyyy-MM-dd") ?? "";
                        hoja.Cell(row, 13).Value = f.HastaElDia?.ToString("yyyy-MM-dd") ?? "";
                        hoja.Cell(row, 14).Value = f.FechaDeSolicitud.ToString("yyyy-MM-dd HH:mm");
                        hoja.Cell(row, 15).Value = f.FechaDeAutorizacion?.ToString("yyyy-MM-dd HH:mm") ?? "";
                        hoja.Cell(row, 16).Value = f.IdSolicitante;
                        hoja.Cell(row, 17).Value = f.CorreoDelSolicitante;
                        row++;
                    }
                    hoja.Style.Font.FontName = "Arial";
                    hoja.Columns().AdjustToContents();
                }

                AgregarHojaCatalogo(workbook, "Tipos de Permiso",
                    _context.PermissionTypes.Select(t => new { t.Id, t.Description }).ToList().Select(t => (t.Id, t.Description)),
                    XLColor.LightYellow);
                AgregarHojaCatalogo(workbook, "Áreas Dentro Del Plantel",
                    _context.Areas.ToList().Select(a => (a.Id, a.Description)),
                    XLColor.LightGray);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        private static void AgregarHojaCatalogo(XLWorkbook workbook, string nombreHoja, IEnumerable<(int Id, string Descripcion)> datos, XLColor color)
        {
            var lista = datos.ToList();
            if (!lista.Any()) return;

            var hoja = workbook.Worksheets.Add(nombreHoja);
            hoja.Cell(1, 1).Value = "ID";
            hoja.Cell(1, 2).Value = "DESCRIPCIÓN";
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 2).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Fill.BackgroundColor = color;
            hoja.Cell(1, 2).Style.Fill.BackgroundColor = color;

            int row = 2;
            foreach (var d in lista)
            {
                hoja.Cell(row, 1).Value = d.Id;
                hoja.Cell(row, 2).Value = d.Descripcion;
                row++;
            }
            hoja.Style.Font.FontName = "Arial";
            hoja.Columns().AdjustToContents();
        }

        public async Task<(byte[] Content, string FileName)> GenerateCutoffCsvAsync(int cutoffId)
        {
            if (!await _context.Cutoffs.AnyAsync(c => c.Id == cutoffId)) return (null!, null!);

            var csv = new StringBuilder();
            csv.AppendLine("ID,Solicitante,Área,Tipo de Permiso,Motivo,Días Solicitados,Con Goce,Evidencia Adjunta,Estado,Status,Autorizado Por,Desde el Día,Hasta el Día,Fecha de Solicitud,Fecha de Autorización,ID Solicitante,Correo");

            foreach (var f in await BuildRowsAsync(cutoffId))
            {
                csv.AppendLine(
                    $"{f.IdSolicitud}," +
                    $"\"{f.Solicitante}\"," +
                    $"\"{f.Area}\"," +
                    $"\"{f.TipoPermiso}\"," +
                    $"\"{f.Motivo.Replace("\"", "\"\"")}\"," +
                    $"{f.DiasSolicitados}," +
                    $"{f.ConGoce}," +
                    $"{f.EvidenciaAdjunta}," +
                    $"\"{f.Estado}\"," +
                    $"{f.Status}," +
                    $"\"{f.AutorizadoPor}\"," +
                    $"{f.DesdeElDia?.ToString("yyyy-MM-dd")}," +
                    $"{f.HastaElDia?.ToString("yyyy-MM-dd")}," +
                    $"{f.FechaDeSolicitud:yyyy-MM-dd HH:mm}," +
                    $"{f.FechaDeAutorizacion?.ToString("yyyy-MM-dd HH:mm")}," +
                    $"{f.IdSolicitante}," +
                    $"\"{f.CorreoDelSolicitante}\"");
            }

            return (Encoding.UTF8.GetBytes(csv.ToString()), $"CorteTEC - ID {cutoffId}.csv");
        }

        public async Task<(byte[] Content, string FileName)> GenerateCutoffJsonAsync(int cutoffId)
        {
            var cutoff = await _context.Cutoffs
                .FirstOrDefaultAsync(c => c.Id == cutoffId);

            if (cutoff == null) return (null!, null!);

            var filas = await BuildRowsAsync(cutoffId);

            var data = new
            {
                Corte = new
                {
                    cutoff.Id,
                    FechaInicio = cutoff.StartDate.ToString("yyyy-MM-dd"),
                    FechaFin = cutoff.EndDate.ToString("yyyy-MM-dd"),
                    Estado = cutoff.CutoffStatusTypeId
                },
                Permisos = filas.Select(f => new
                {
                    f.IdSolicitud,
                    f.Solicitante,
                    f.Area,
                    f.TipoPermiso,
                    f.Motivo,
                    f.DiasSolicitados,
                    f.ConGoce,
                    f.EvidenciaAdjunta,
                    f.Estado,
                    f.Status,
                    f.AutorizadoPor,
                    DesdeElDia = f.DesdeElDia?.ToString("yyyy-MM-dd"),
                    HastaElDia = f.HastaElDia?.ToString("yyyy-MM-dd"),
                    f.FechaDeSolicitud,
                    f.FechaDeAutorizacion,
                    f.IdSolicitante,
                    f.CorreoDelSolicitante
                }),
                Estadisticas = new
                {
                    Total = filas.Count,
                    Revisados = filas.Count(f => f.Estado.Contains("Revisado")),
                    Pendientes = filas.Count(f => f.Estado.Contains("Pendiente")),
                    DiasPromedio = filas.Any() ? filas.Average(f => f.DiasSolicitados) : 0
                }
            };

            var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return (Encoding.UTF8.GetBytes(json), $"CorteTEC - ID {cutoffId}.json");
        }

        public async Task<(byte[] Content, string FileName)> GenerateCutoffXmlAsync(int cutoffId)
        {
            var cutoff = await _context.Cutoffs
                .FirstOrDefaultAsync(c => c.Id == cutoffId);

            if (cutoff == null) return (null!, null!);

            var filas = await BuildRowsAsync(cutoffId);

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Corte");

                xmlWriter.WriteElementString("Id", cutoff.Id.ToString());
                xmlWriter.WriteElementString("FechaInicio", cutoff.StartDate.ToString("yyyy-MM-dd"));
                xmlWriter.WriteElementString("FechaFin", cutoff.EndDate.ToString("yyyy-MM-dd"));
                xmlWriter.WriteElementString("Estado", cutoff.CutoffStatusTypeId.ToString());

                xmlWriter.WriteStartElement("Estadisticas");
                xmlWriter.WriteElementString("Total", filas.Count.ToString());
                xmlWriter.WriteElementString("Revisados", filas.Count(f => f.Estado.Contains("Revisado")).ToString());
                xmlWriter.WriteElementString("Pendientes", filas.Count(f => f.Estado.Contains("Pendiente")).ToString());
                xmlWriter.WriteElementString("DiasPromedio", (filas.Any() ? filas.Average(f => f.DiasSolicitados) : 0).ToString("F2"));
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Permisos");
                foreach (var f in filas)
                {
                    xmlWriter.WriteStartElement("Permiso");
                    xmlWriter.WriteElementString("IdSolicitud", f.IdSolicitud.ToString());
                    xmlWriter.WriteElementString("Solicitante", f.Solicitante);
                    xmlWriter.WriteElementString("Area", f.Area);
                    xmlWriter.WriteElementString("TipoPermiso", f.TipoPermiso);
                    xmlWriter.WriteElementString("Motivo", f.Motivo);
                    xmlWriter.WriteElementString("DiasSolicitados", f.DiasSolicitados.ToString());
                    xmlWriter.WriteElementString("ConGoce", f.ConGoce);
                    xmlWriter.WriteElementString("EvidenciaAdjunta", f.EvidenciaAdjunta);
                    xmlWriter.WriteElementString("Estado", f.Estado);
                    xmlWriter.WriteElementString("Status", f.Status.ToString());
                    xmlWriter.WriteElementString("AutorizadoPor", f.AutorizadoPor);
                    xmlWriter.WriteElementString("DesdeElDia", f.DesdeElDia?.ToString("yyyy-MM-dd") ?? "");
                    xmlWriter.WriteElementString("HastaElDia", f.HastaElDia?.ToString("yyyy-MM-dd") ?? "");
                    xmlWriter.WriteElementString("FechaDeSolicitud", f.FechaDeSolicitud.ToString("yyyy-MM-dd HH:mm"));
                    xmlWriter.WriteElementString("FechaDeAutorizacion", f.FechaDeAutorizacion?.ToString("yyyy-MM-dd HH:mm") ?? "");
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();

                return (Encoding.UTF8.GetBytes(stringWriter.ToString()), $"CorteTEC - ID {cutoffId}.xml");
            }
        }
    }
}
