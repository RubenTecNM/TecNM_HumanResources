using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RecursosHumanosWeb.Models;
using System.Text;
using System.Xml;

namespace RecursosHumanosWeb.Services
{
    public class ExportService : IExportService
    {
        private readonly RecursosHumanosContext _context;

        public ExportService(RecursosHumanosContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateCorteExcelAsync(int corteId)
        {
            var corte = await _context.Cortes
                .Include(c => c.IdUsuarioCreaNavigation)
                .FirstOrDefaultAsync(c => c.Id == corteId);

            if (corte == null) return null;

            var areas = await _context.Areas
                .OrderBy(a => a.Descripcion)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                foreach (var area in areas)
                {
                    var permisosGenerales = await _context.Permisos
                        .Where(p => p.IdCorte == corteId &&
                                    p.IdUsuarioSolicitaNavigation.IdArea == area.Id &&
                                    p.IdTipoPermiso != 5 &&
                                    p.Estatus != false)
                        .Include(p => p.IdUsuarioSolicitaNavigation)
                        .Include(p => p.IdUsuarioAutorizaNavigation)
                        .Include(p => p.IdTipoPermisoNavigation)
                        .Select(p => new
                        {
                            ID_SOLICITUD = p.Id,
                            NOMBRE_DEL_SOLICITANTE = p.IdUsuarioSolicitaNavigation.Nombre,
                            ID_TIPO_PERMISO = p.IdTipoPermiso,
                            TIPO_PERMISO = p.IdTipoPermisoNavigation.Descripcion,
                            DESCRIPCIÓN = p.Motivo ?? "",
                            DÍAS_SOLICITADOS = p.Fecha1.HasValue && p.Fecha2.HasValue 
                                ? (int)((p.Fecha2.Value - p.Fecha1.Value).TotalDays + 1)
                                : (p.Dias ?? 0),
                            CON_GOCE = p.Goce ? "Sí" : "No",
                            EVIDENCIA = p.Evidencia ?? "",
                            DESDE_EL_DÍA = p.Fecha1,
                            HASTA_EL_DÍA = p.Fecha2,
                            FECHA_DE_SOLICITUD = p.FechaCreacion,
                            FECHA_DE_AUTORIZACIÓN = p.FechaAutorizacion,
                            ID_SOLICITANTE = p.IdUsuarioSolicita,
                            CORREO_DEL_SOLICITANTE = p.IdUsuarioSolicitaNavigation.Correo
                        })
                        .ToListAsync();

                    if (permisosGenerales.Any())
                    {
                        var worksheetGeneral = workbook.Worksheets.Add($"Área - {area.Id}");
                        var headers = new[] {
                            "ID_SOLICITUD", "NOMBRE_DEL_SOLICITANTE", "ID_TIPO_PERMISO",
                            "TIPO_PERMISO", "DESCRIPCIÓN", "DÍAS_SOLICITADOS", "CON_GOCE",
                            "EVIDENCIA", "DESDE_EL_DÍA", "HASTA_EL_DÍA",
                            "FECHA_DE_SOLICITUD", "FECHA_DE_AUTORIZACIÓN",
                            "ID_SOLICITANTE", "CORREO_DEL_SOLICITANTE"
                        };

                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheetGeneral.Cell(1, i + 1).Value = headers[i];
                            worksheetGeneral.Cell(1, i + 1).Style.Font.Bold = true;
                            worksheetGeneral.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                        }

                        int row = 2;
                        foreach (var permiso in permisosGenerales)
                        {
                            worksheetGeneral.Cell(row, 1).Value = permiso.ID_SOLICITUD;
                            worksheetGeneral.Cell(row, 2).Value = permiso.NOMBRE_DEL_SOLICITANTE;
                            worksheetGeneral.Cell(row, 3).Value = permiso.ID_TIPO_PERMISO;
                            worksheetGeneral.Cell(row, 4).Value = permiso.TIPO_PERMISO;
                            worksheetGeneral.Cell(row, 5).Value = permiso.DESCRIPCIÓN;
                            worksheetGeneral.Cell(row, 6).Value = permiso.DÍAS_SOLICITADOS;
                            worksheetGeneral.Cell(row, 7).Value = permiso.CON_GOCE;
                            worksheetGeneral.Cell(row, 8).Value = permiso.EVIDENCIA;
                            worksheetGeneral.Cell(row, 9).Value = permiso.DESDE_EL_DÍA;
                            worksheetGeneral.Cell(row, 10).Value = permiso.HASTA_EL_DÍA;
                            worksheetGeneral.Cell(row, 11).Value = permiso.FECHA_DE_SOLICITUD;
                            worksheetGeneral.Cell(row, 12).Value = permiso.FECHA_DE_AUTORIZACIÓN;
                            worksheetGeneral.Cell(row, 13).Value = permiso.ID_SOLICITANTE;
                            worksheetGeneral.Cell(row, 14).Value = permiso.CORREO_DEL_SOLICITANTE;
                            row++;
                        }
                        worksheetGeneral.Style.Font.FontName = "Arial";
                        worksheetGeneral.Columns().AdjustToContents();
                    }

                    var permisosTurno = await _context.Permisos
                        .Where(p => p.IdCorte == corteId &&
                                    p.IdUsuarioSolicitaNavigation.IdArea == area.Id &&
                                    p.IdTipoPermiso == 5 &&
                                    p.Estatus != false)
                        .Include(p => p.IdUsuarioSolicitaNavigation)
                        .Include(p => p.IdUsuarioAutorizaNavigation)
                        .Include(p => p.IdTipoPermisoNavigation)
                        .Select(p => new
                        {
                            ID_SOLICITUD = p.Id,
                            NOMBRE_DEL_SOLICITANTE = p.IdUsuarioSolicitaNavigation.Nombre,
                            ID_TIPO_PERMISO = p.IdTipoPermiso,
                            TIPO_PERMISO = p.IdTipoPermisoNavigation.Descripcion,
                            DESCRIPCIÓN = p.Motivo ?? "",
                            DÍAS_SOLICITADOS = p.Dias ?? 0,
                            CON_GOCE = p.Goce ? "Sí" : "No",
                            EVIDENCIA = p.Evidencia ?? "",
                            DEL_DÍA = p.Fecha1,
                            POR_EL_DÍA = p.Fecha2,
                            FECHA_DE_SOLICITUD = p.FechaCreacion,
                            FECHA_DE_AUTORIZACIÓN = p.FechaAutorizacion,
                            ID_SOLICITANTE = p.IdUsuarioSolicita,
                            CORREO_DEL_SOLICITANTE = p.IdUsuarioSolicitaNavigation.Correo
                        })
                        .ToListAsync();

                    if (permisosTurno.Any())
                    {
                        var worksheetTurno = workbook.Worksheets.Add($"{area.Id} - Turno por Turno");
                        var headersTurno = new[] {
                            "ID_SOLICITUD", "NOMBRE_DEL_SOLICITANTE", "ID_TIPO_PERMISO",
                            "TIPO_PERMISO", "DESCRIPCIÓN", "DÍAS_SOLICITADOS", "CON_GOCE",
                            "EVIDENCIA", "DEL_DÍA", "POR_EL_DÍA",
                            "FECHA_DE_SOLICITUD", "FECHA_DE_AUTORIZACIÓN",
                            "ID_SOLICITANTE", "CORREO_DEL_SOLICITANTE"
                        };

                        for (int i = 0; i < headersTurno.Length; i++)
                        {
                            worksheetTurno.Cell(1, i + 1).Value = headersTurno[i];
                            worksheetTurno.Cell(1, i + 1).Style.Font.Bold = true;
                            worksheetTurno.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }

                        int row = 2;
                        foreach (var permiso in permisosTurno)
                        {
                            worksheetTurno.Cell(row, 1).Value = permiso.ID_SOLICITUD;
                            worksheetTurno.Cell(row, 2).Value = permiso.NOMBRE_DEL_SOLICITANTE;
                            worksheetTurno.Cell(row, 3).Value = permiso.ID_TIPO_PERMISO;
                            worksheetTurno.Cell(row, 4).Value = permiso.TIPO_PERMISO;
                            worksheetTurno.Cell(row, 5).Value = permiso.DESCRIPCIÓN;
                            worksheetTurno.Cell(row, 6).Value = permiso.DÍAS_SOLICITADOS;
                            worksheetTurno.Cell(row, 7).Value = permiso.CON_GOCE;
                            worksheetTurno.Cell(row, 8).Value = permiso.EVIDENCIA;
                            worksheetTurno.Cell(row, 9).Value = permiso.DEL_DÍA;
                            worksheetTurno.Cell(row, 10).Value = permiso.POR_EL_DÍA;
                            worksheetTurno.Cell(row, 11).Value = permiso.FECHA_DE_SOLICITUD;
                            worksheetTurno.Cell(row, 12).Value = permiso.FECHA_DE_AUTORIZACIÓN;
                            worksheetTurno.Cell(row, 13).Value = permiso.ID_SOLICITANTE;
                            worksheetTurno.Cell(row, 14).Value = permiso.CORREO_DEL_SOLICITANTE;
                            row++;
                        }
                        worksheetTurno.Style.Font.FontName = "Arial";
                        worksheetTurno.Columns().AdjustToContents();
                    }
                }

                // Tipos de Permisos
                var tiposPermisos = await _context.TipoPermisos
                    .Select(tp => new { ID_TIPO_PERMISO = tp.Id, DESCRIPCIÓN = tp.Descripcion })
                    .ToListAsync();

                if (tiposPermisos.Any())
                {
                    var worksheetTipos = workbook.Worksheets.Add("Tipos De Permisos");
                    worksheetTipos.Cell(1, 1).Value = "ID_TIPO_PERMISO";
                    worksheetTipos.Cell(1, 2).Value = "DESCRIPCIÓN";
                    worksheetTipos.Cell(1, 1).Style.Font.Bold = true;
                    worksheetTipos.Cell(1, 2).Style.Font.Bold = true;
                    worksheetTipos.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
                    worksheetTipos.Cell(1, 2).Style.Fill.BackgroundColor = XLColor.LightYellow;

                    int row = 2;
                    foreach (var tipo in tiposPermisos)
                    {
                        worksheetTipos.Cell(row, 1).Value = tipo.ID_TIPO_PERMISO;
                        worksheetTipos.Cell(row, 2).Value = tipo.DESCRIPCIÓN;
                        row++;
                    }
                    worksheetTipos.Style.Font.FontName = "Arial";
                    worksheetTipos.Style.Font.FontSize = 16;
                    worksheetTipos.Columns().AdjustToContents();
                }

                // Areas
                var areasData = await _context.Areas
                    .Select(a => new { ID = a.Id, DESCRIPCIÓN = a.Descripcion })
                    .ToListAsync();

                if (areasData.Any())
                {
                    var worksheetAreas = workbook.Worksheets.Add("Áreas Dentro Del Plantel");
                    worksheetAreas.Cell(1, 1).Value = "ID";
                    worksheetAreas.Cell(1, 2).Value = "DESCRIPCIÓN";
                    worksheetAreas.Cell(1, 1).Style.Font.Bold = true;
                    worksheetAreas.Cell(1, 2).Style.Font.Bold = true;
                    worksheetAreas.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheetAreas.Cell(1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

                    int row = 2;
                    foreach (var areaData in areasData)
                    {
                        worksheetAreas.Cell(row, 1).Value = areaData.ID;
                        worksheetAreas.Cell(row, 2).Value = areaData.DESCRIPCIÓN;
                        row++;
                    }
                    worksheetAreas.Style.Font.FontName = "Arial";
                    worksheetAreas.Style.Font.FontSize = 16;
                    worksheetAreas.Columns().AdjustToContents();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<(byte[] Content, string FileName)> GenerateCorteCsvAsync(int corteId)
        {
            var corte = await _context.Cortes
                .Include(c => c.IdUsuarioCreaNavigation)
                .FirstOrDefaultAsync(c => c.Id == corteId);

            if (corte == null) return (null, null);

            var permisos = await _context.Permisos
                .Where(p => p.IdCorte == corteId && p.Estatus != false)
                .Include(p => p.IdUsuarioSolicitaNavigation)
                    .ThenInclude(u => u.IdAreaNavigation)
                .Include(p => p.IdUsuarioAutorizaNavigation)
                .Include(p => p.IdTipoPermisoNavigation)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("ID,Solicitante,Área,Tipo Permiso,Motivo,Días,Con Goce,Del Día,Al Día,Revisado,Autorizado Por,Fecha Creación");

            foreach (var p in permisos)
            {
                csv.AppendLine($"{p.Id}," +
                    $"\"{p.IdUsuarioSolicitaNavigation?.Nombre}\"," +
                    $"\"{p.IdUsuarioSolicitaNavigation?.IdAreaNavigation?.Descripcion}\"," +
                    $"\"{p.IdTipoPermisoNavigation?.Descripcion}\"," +
                    $"\"{p.Motivo?.Replace("\"", "\"\"")}\"," +
                    $"{p.Dias ?? 0}," +
                    $"{(p.Goce ? "Sí" : "No")}," +
                    $"{p.Fecha1?.ToString("yyyy-MM-dd")}," +
                    $"{p.Fecha2?.ToString("yyyy-MM-dd")}," +
                    $"{(p.Revisado ? "Sí" : "No")}," +
                    $"\"{p.IdUsuarioAutorizaNavigation?.Nombre}\"," +
                    $"{p.FechaCreacion.ToString("yyyy-MM-dd HH:mm")}");
            }

            string fileName = $"Corte_{corte.Inicia.ToString("yyyyMMdd") ?? "SinFecha"}.csv";
            return (Encoding.UTF8.GetBytes(csv.ToString()), fileName);
        }

        public async Task<(byte[] Content, string FileName)> GenerateCorteJsonAsync(int corteId)
        {
            var corte = await _context.Cortes
                .Include(c => c.IdUsuarioCreaNavigation)
                .FirstOrDefaultAsync(c => c.Id == corteId);

            if (corte == null) return (null, null);

            var permisos = await _context.Permisos
                .Where(p => p.IdCorte == corteId && p.Estatus != false)
                .Include(p => p.IdUsuarioSolicitaNavigation)
                    .ThenInclude(u => u.IdAreaNavigation)
                .Include(p => p.IdUsuarioAutorizaNavigation)
                .Include(p => p.IdTipoPermisoNavigation)
                .Select(p => new
                {
                    Id = p.Id,
                    Solicitante = p.IdUsuarioSolicitaNavigation != null ? (p.IdUsuarioSolicitaNavigation.Nombre ?? "") : "",
                    Area = p.IdUsuarioSolicitaNavigation != null && p.IdUsuarioSolicitaNavigation.IdAreaNavigation != null ? (p.IdUsuarioSolicitaNavigation.IdAreaNavigation.Descripcion ?? "") : "",
                    TipoPermiso = p.IdTipoPermisoNavigation != null ? (p.IdTipoPermisoNavigation.Descripcion ?? "") : "",
                    Motivo = p.Motivo,
                    Dias = p.Dias,
                    ConGoce = p.Goce,
                    DelDia = p.Fecha1,
                    AlDia = p.Fecha2,
                    Revisado = p.Revisado,
                    AutorizadoPor = p.IdUsuarioAutorizaNavigation != null ? p.IdUsuarioAutorizaNavigation.Nombre : null,
                    FechaCreacion = p.FechaCreacion
                })
                .ToListAsync();

            var data = new
            {
                Corte = new
                {
                    Id = corte.Id,
                    FechaInicio = corte.Inicia,
                    FechaFin = corte.Termina,
                    Estatus = corte.Estatus
                },
                Permisos = permisos,
                Estadisticas = new
                {
                    Total = permisos.Count,
                    Aprobados = permisos.Count(p => p.Revisado ),
                    Pendientes = permisos.Count(p => !p.Revisado),
                    DiasPromedio = permisos.Any() ? permisos.Average(p => p.Dias ?? 0) : 0
                }
            };

            var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            string fileName = $"Corte_{corte.Inicia.ToString("yyyyMMdd") ?? "SinFecha"}.json";
            return (Encoding.UTF8.GetBytes(json), fileName);
        }

        public async Task<(byte[] Content, string FileName)> GenerateCorteXmlAsync(int corteId)
        {
            var corte = await _context.Cortes
                .Include(c => c.IdUsuarioCreaNavigation)
                .FirstOrDefaultAsync(c => c.Id == corteId);

            if (corte == null) return (null, null);

            var permisos = await _context.Permisos
                .Where(p => p.IdCorte == corteId && p.Estatus != false)
                .Include(p => p.IdUsuarioSolicitaNavigation)
                    .ThenInclude(u => u.IdAreaNavigation)
                .Include(p => p.IdUsuarioAutorizaNavigation)
                .Include(p => p.IdTipoPermisoNavigation)
                .ToListAsync();

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Corte");
                
                xmlWriter.WriteElementString("Id", corte.Id.ToString());
                xmlWriter.WriteElementString("FechaInicio", corte.Inicia.ToString("yyyy-MM-dd") ?? "");
                xmlWriter.WriteElementString("FechaFin", corte.Termina.ToString("yyyy-MM-dd") ?? "");
                xmlWriter.WriteElementString("Estatus", corte.Estatus.ToString());

                xmlWriter.WriteStartElement("Estadisticas");
                xmlWriter.WriteElementString("Total", permisos.Count.ToString());
                xmlWriter.WriteElementString("Aprobados", permisos.Count(p => p.Revisado).ToString());
                xmlWriter.WriteElementString("Pendientes", permisos.Count(p => !p.Revisado).ToString());
                xmlWriter.WriteElementString("DiasPromedio", (permisos.Any() ? permisos.Average(p => p.Dias ?? 0) : 0).ToString("F2"));
                xmlWriter.WriteEndElement(); 

                xmlWriter.WriteStartElement("Permisos");
                foreach (var p in permisos)
                {
                    xmlWriter.WriteStartElement("Permiso");
                    xmlWriter.WriteElementString("Id", p.Id.ToString());
                    xmlWriter.WriteElementString("Solicitante", p.IdUsuarioSolicitaNavigation?.Nombre ?? "");
                    xmlWriter.WriteElementString("Area", p.IdUsuarioSolicitaNavigation?.IdAreaNavigation?.Descripcion ?? "");
                    xmlWriter.WriteElementString("TipoPermiso", p.IdTipoPermisoNavigation?.Descripcion ?? "");
                    xmlWriter.WriteElementString("Motivo", p.Motivo ?? "");
                    xmlWriter.WriteElementString("Dias", (p.Dias ?? 0).ToString());
                    xmlWriter.WriteElementString("ConGoce", p.Goce ? "Sí" : "No");
                    xmlWriter.WriteElementString("DelDia", p.Fecha1?.ToString("yyyy-MM-dd") ?? "");
                    xmlWriter.WriteElementString("AlDia", p.Fecha2?.ToString("yyyy-MM-dd") ?? "");
                    xmlWriter.WriteElementString("Revisado", p.Revisado ? "Sí" : "No");
                    xmlWriter.WriteElementString("AutorizadoPor", p.IdUsuarioAutorizaNavigation?.Nombre ?? "");
                    xmlWriter.WriteElementString("FechaCreacion", p.FechaCreacion.ToString("yyyy-MM-dd HH:mm") ?? "");
                    xmlWriter.WriteEndElement(); 
                }
                xmlWriter.WriteEndElement(); 

                xmlWriter.WriteEndElement(); 
                xmlWriter.WriteEndDocument();

                string fileName = $"Corte_{corte.Inicia.ToString("yyyyMMdd") ?? "SinFecha"}.xml";
                return (Encoding.UTF8.GetBytes(stringWriter.ToString()), fileName);
            }
        }
    }
}
