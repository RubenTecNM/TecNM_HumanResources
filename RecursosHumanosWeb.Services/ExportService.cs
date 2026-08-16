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

        public async Task<byte[]> GenerateCutoffExcelAsync(int cutoffId)
        {
            var cutoff = await _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == cutoffId);

            if (cutoff == null) return null;

            var areas = await _context.Areas
                .OrderBy(a => a.Description)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                foreach (var area in areas)
                {
                    var generalPermissions = await _context.Permissions
                        .Where(p => p.CutoffId == cutoffId &&
                                    p.RequestingUser.AreaId == area.Id &&
                                    p.PermissionTypeId != 5 &&
                                    p.PermissionStatusTypeId != (int)PermissionStatusEnum.InvalidatedByPeriod) // Excluyendo Invalidados
                        .Include(p => p.RequestingUser)
                        .Include(p => p.AuthorizingUser)
                        .Include(p => p.PermissionType)
                        .Select(p => new
                        {
                            ID_SOLICITUD = p.Id,
                            NOMBRE_DEL_SOLICITANTE = p.RequestingUser.Name,
                            ID_TIPO_PERMISO = p.PermissionTypeId,
                            TIPO_PERMISO = p.PermissionType.Description,
                            DESCRIPCIÓN = p.Reason ?? "",
                            DÍAS_SOLICITADOS = p.StartDate.HasValue && p.EndDate.HasValue 
                                ? (int)((p.EndDate.Value - p.StartDate.Value).TotalDays + 1)
                                : (p.Days ?? 0),
                            CON_GOCE = p.WithPay ? "Sí" : "No",
                            EVIDENCIA = p.Evidence ?? "",
                            DESDE_EL_DÍA = p.StartDate,
                            HASTA_EL_DÍA = p.EndDate,
                            FECHA_DE_SOLICITUD = p.CreatedAt,
                            FECHA_DE_AUTORIZACIÓN = p.AuthorizedAt,
                            ID_SOLICITANTE = p.RequestingUserId,
                            CORREO_DEL_SOLICITANTE = p.RequestingUser.Email
                        })
                        .ToListAsync();

                    if (generalPermissions.Any())
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
                        foreach (var permission in generalPermissions)
                        {
                            worksheetGeneral.Cell(row, 1).Value = permission.ID_SOLICITUD;
                            worksheetGeneral.Cell(row, 2).Value = permission.NOMBRE_DEL_SOLICITANTE;
                            worksheetGeneral.Cell(row, 3).Value = permission.ID_TIPO_PERMISO;
                            worksheetGeneral.Cell(row, 4).Value = permission.TIPO_PERMISO;
                            worksheetGeneral.Cell(row, 5).Value = permission.DESCRIPCIÓN;
                            worksheetGeneral.Cell(row, 6).Value = permission.DÍAS_SOLICITADOS;
                            worksheetGeneral.Cell(row, 7).Value = permission.CON_GOCE;
                            worksheetGeneral.Cell(row, 8).Value = permission.EVIDENCIA;
                            worksheetGeneral.Cell(row, 9).Value = permission.DESDE_EL_DÍA;
                            worksheetGeneral.Cell(row, 10).Value = permission.HASTA_EL_DÍA;
                            worksheetGeneral.Cell(row, 11).Value = permission.FECHA_DE_SOLICITUD;
                            worksheetGeneral.Cell(row, 12).Value = permission.FECHA_DE_AUTORIZACIÓN;
                            worksheetGeneral.Cell(row, 13).Value = permission.ID_SOLICITANTE;
                            worksheetGeneral.Cell(row, 14).Value = permission.CORREO_DEL_SOLICITANTE;
                            row++;
                        }
                        worksheetGeneral.Style.Font.FontName = "Arial";
                        worksheetGeneral.Columns().AdjustToContents();
                    }

                    var shiftPermissions = await _context.Permissions
                        .Where(p => p.CutoffId == cutoffId &&
                                    p.RequestingUser.AreaId == area.Id &&
                                    p.PermissionTypeId == 5 &&
                                    p.PermissionStatusTypeId != (int)PermissionStatusEnum.InvalidatedByPeriod)
                        .Include(p => p.RequestingUser)
                        .Include(p => p.AuthorizingUser)
                        .Include(p => p.PermissionType)
                        .Select(p => new
                        {
                            ID_SOLICITUD = p.Id,
                            NOMBRE_DEL_SOLICITANTE = p.RequestingUser.Name,
                            ID_TIPO_PERMISO = p.PermissionTypeId,
                            TIPO_PERMISO = p.PermissionType.Description,
                            DESCRIPCIÓN = p.Reason ?? "",
                            DÍAS_SOLICITADOS = p.Days ?? 0,
                            CON_GOCE = p.WithPay ? "Sí" : "No",
                            EVIDENCIA = p.Evidence ?? "",
                            DEL_DÍA = p.StartDate,
                            POR_EL_DÍA = p.EndDate,
                            FECHA_DE_SOLICITUD = p.CreatedAt,
                            FECHA_DE_AUTORIZACIÓN = p.AuthorizedAt,
                            ID_SOLICITANTE = p.RequestingUserId,
                            CORREO_DEL_SOLICITANTE = p.RequestingUser.Email
                        })
                        .ToListAsync();

                    if (shiftPermissions.Any())
                    {
                        var worksheetShift = workbook.Worksheets.Add($"{area.Id} - Turno por Turno");
                        var headersTurno = new[] {
                            "ID_SOLICITUD", "NOMBRE_DEL_SOLICITANTE", "ID_TIPO_PERMISO",
                            "TIPO_PERMISO", "DESCRIPCIÓN", "DÍAS_SOLICITADOS", "CON_GOCE",
                            "EVIDENCIA", "DEL_DÍA", "POR_EL_DÍA",
                            "FECHA_DE_SOLICITUD", "FECHA_DE_AUTORIZACIÓN",
                            "ID_SOLICITANTE", "CORREO_DEL_SOLICITANTE"
                        };

                        for (int i = 0; i < headersTurno.Length; i++)
                        {
                            worksheetShift.Cell(1, i + 1).Value = headersTurno[i];
                            worksheetShift.Cell(1, i + 1).Style.Font.Bold = true;
                            worksheetShift.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }

                        int row = 2;
                        foreach (var permission in shiftPermissions)
                        {
                            worksheetShift.Cell(row, 1).Value = permission.ID_SOLICITUD;
                            worksheetShift.Cell(row, 2).Value = permission.NOMBRE_DEL_SOLICITANTE;
                            worksheetShift.Cell(row, 3).Value = permission.ID_TIPO_PERMISO;
                            worksheetShift.Cell(row, 4).Value = permission.TIPO_PERMISO;
                            worksheetShift.Cell(row, 5).Value = permission.DESCRIPCIÓN;
                            worksheetShift.Cell(row, 6).Value = permission.DÍAS_SOLICITADOS;
                            worksheetShift.Cell(row, 7).Value = permission.CON_GOCE;
                            worksheetShift.Cell(row, 8).Value = permission.EVIDENCIA;
                            worksheetShift.Cell(row, 9).Value = permission.DEL_DÍA;
                            worksheetShift.Cell(row, 10).Value = permission.POR_EL_DÍA;
                            worksheetShift.Cell(row, 11).Value = permission.FECHA_DE_SOLICITUD;
                            worksheetShift.Cell(row, 12).Value = permission.FECHA_DE_AUTORIZACIÓN;
                            worksheetShift.Cell(row, 13).Value = permission.ID_SOLICITANTE;
                            worksheetShift.Cell(row, 14).Value = permission.CORREO_DEL_SOLICITANTE;
                            row++;
                        }
                        worksheetShift.Style.Font.FontName = "Arial";
                        worksheetShift.Columns().AdjustToContents();
                    }
                }

                // Tipos de Permissions
                var permissionTypes = await _context.PermissionTypes
                    .Select(tp => new { ID_TIPO_PERMISO = tp.Id, DESCRIPCIÓN = tp.Description })
                    .ToListAsync();

                if (permissionTypes.Any())
                {
                    var worksheetTypes = workbook.Worksheets.Add("Tipos De Permissions");
                    worksheetTypes.Cell(1, 1).Value = "ID_TIPO_PERMISO";
                    worksheetTypes.Cell(1, 2).Value = "DESCRIPCIÓN";
                    worksheetTypes.Cell(1, 1).Style.Font.Bold = true;
                    worksheetTypes.Cell(1, 2).Style.Font.Bold = true;
                    worksheetTypes.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
                    worksheetTypes.Cell(1, 2).Style.Fill.BackgroundColor = XLColor.LightYellow;

                    int row = 2;
                    foreach (var tipo in permissionTypes)
                    {
                        worksheetTypes.Cell(row, 1).Value = tipo.ID_TIPO_PERMISO;
                        worksheetTypes.Cell(row, 2).Value = tipo.DESCRIPCIÓN;
                        row++;
                    }
                    worksheetTypes.Style.Font.FontName = "Arial";
                    worksheetTypes.Style.Font.FontSize = 16;
                    worksheetTypes.Columns().AdjustToContents();
                }

                // Areas
                var areasData = await _context.Areas
                    .Select(a => new { ID = a.Id, DESCRIPCIÓN = a.Description })
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

        public async Task<(byte[] Content, string FileName)> GenerateCutoffCsvAsync(int cutoffId)
        {
            var cutoff = await _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == cutoffId);

            if (cutoff == null) return (null, null);

            var permissions = await _context.Permissions
                .Where(p => p.CutoffId == cutoffId && p.PermissionStatusTypeId != (int)PermissionStatusEnum.InvalidatedByPeriod)
                .Include(p => p.RequestingUser)
                    .ThenInclude(u => u.Area)
                .Include(p => p.AuthorizingUser)
                .Include(p => p.PermissionType)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("ID,Solicitante,Área,Tipo Permission,Reason,Días,Con WithPay,Del Día,Al Día,Estado,Autorizado Por,Fecha Creación");

            foreach (var p in permissions)
            {
                csv.AppendLine($"{p.Id}," +
                    $"\"{p.RequestingUser?.Name}\"," +
                    $"\"{p.RequestingUser?.Area?.Description}\"," +
                    $"\"{p.PermissionType?.Description}\"," +
                    $"\"{p.Reason?.Replace("\"", "\"\"")}\"," +
                    $"{p.Days ?? 0}," +
                    $"{(p.WithPay ? "Sí" : "No")}," +
                    $"{p.StartDate?.ToString("yyyy-MM-dd")}," +
                    $"{p.EndDate?.ToString("yyyy-MM-dd")}," +
                    $"{(p.PermissionStatusTypeId == (int)PermissionStatusEnum.Approved ? "Sí" : "No")}," +
                    $"\"{p.AuthorizingUser?.Name}\"," +
                    $"{p.CreatedAt.ToString("yyyy-MM-dd HH:mm")}");
            }

            string fileName = $"CorteTEC - ID {cutoffId}.csv";
            return (Encoding.UTF8.GetBytes(csv.ToString()), fileName);
        }

        public async Task<(byte[] Content, string FileName)> GenerateCutoffJsonAsync(int cutoffId)
        {
            var cutoff = await _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == cutoffId);

            if (cutoff == null) return (null, null);

            var permissions = await _context.Permissions
                .Where(p => p.CutoffId == cutoffId && p.PermissionStatusTypeId != (int)PermissionStatusEnum.InvalidatedByPeriod)
                .Include(p => p.RequestingUser)
                    .ThenInclude(u => u.Area)
                .Include(p => p.AuthorizingUser)
                .Include(p => p.PermissionType)
                .Select(p => new
                {
                    Id = p.Id,
                    Solicitante = p.RequestingUser != null ? (p.RequestingUser.Name ?? "") : "",
                    Area = p.RequestingUser != null && p.RequestingUser.Area != null ? (p.RequestingUser.Area.Description ?? "") : "",
                    PermissionType = p.PermissionType != null ? (p.PermissionType.Description ?? "") : "",
                    Reason = p.Reason,
                    Days = p.Days,
                    WithPay = p.WithPay,
                    FromDay = p.StartDate,
                    ToDay = p.EndDate,
                    PermissionStatusTypeId = p.PermissionStatusTypeId,
                    AuthorizedBy = p.AuthorizingUser != null ? p.AuthorizingUser.Name : null,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var data = new
            {
                Cutoff = new
                {
                    Id = cutoff.Id,
                    FechaInicio = cutoff.StartDate,
                    FechaFin = cutoff.EndDate,
                    CutoffStatusTypeId = cutoff.CutoffStatusTypeId
                },
                Permissions = permissions,
                Estadisticas = new
                {
                    Total = permissions.Count,
                    Aprobados = permissions.Count(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Approved),
                    Pendientes = permissions.Count(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending),
                    DiasPromedio = permissions.Any() ? permissions.Average(p => p.Days ?? 0) : 0
                }
            };

            var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            string fileName = $"CorteTEC - ID {cutoffId}.json";
            return (Encoding.UTF8.GetBytes(json), fileName);
        }

        public async Task<(byte[] Content, string FileName)> GenerateCutoffXmlAsync(int cutoffId)
        {
            var cutoff = await _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == cutoffId);

            if (cutoff == null) return (null, null);

            var permissions = await _context.Permissions
                .Where(p => p.CutoffId == cutoffId && p.PermissionStatusTypeId != (int)PermissionStatusEnum.InvalidatedByPeriod)
                .Include(p => p.RequestingUser)
                    .ThenInclude(u => u.Area)
                .Include(p => p.AuthorizingUser)
                .Include(p => p.PermissionType)
                .ToListAsync();

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Cutoff");
                
                xmlWriter.WriteElementString("Id", cutoff.Id.ToString());
                xmlWriter.WriteElementString("FechaInicio", cutoff.StartDate.ToString("yyyy-MM-dd") ?? "");
                xmlWriter.WriteElementString("FechaFin", cutoff.EndDate.ToString("yyyy-MM-dd") ?? "");
                xmlWriter.WriteElementString("CutoffStatusTypeId", cutoff.CutoffStatusTypeId.ToString());

                xmlWriter.WriteStartElement("Estadisticas");
                xmlWriter.WriteElementString("Total", permissions.Count.ToString());
                xmlWriter.WriteElementString("Aprobados", permissions.Count(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Approved).ToString());
                xmlWriter.WriteElementString("Pendientes", permissions.Count(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending).ToString());
                xmlWriter.WriteElementString("DiasPromedio", (permissions.Any() ? permissions.Average(p => p.Days ?? 0) : 0).ToString("F2"));
                xmlWriter.WriteEndElement(); 

                xmlWriter.WriteStartElement("Permissions");
                foreach (var p in permissions)
                {
                    xmlWriter.WriteStartElement("Permission");
                    xmlWriter.WriteElementString("Id", p.Id.ToString());
                    xmlWriter.WriteElementString("Solicitante", p.RequestingUser?.Name ?? "");
                    xmlWriter.WriteElementString("Area", p.RequestingUser?.Area?.Description ?? "");
                    xmlWriter.WriteElementString("PermissionType", p.PermissionType?.Description ?? "");
                    xmlWriter.WriteElementString("Reason", p.Reason ?? "");
                    xmlWriter.WriteElementString("Days", (p.Days ?? 0).ToString());
                    xmlWriter.WriteElementString("WithPay", p.WithPay ? "Sí" : "No");
                    xmlWriter.WriteElementString("FromDay", p.StartDate?.ToString("yyyy-MM-dd") ?? "");
                    xmlWriter.WriteElementString("ToDay", p.EndDate?.ToString("yyyy-MM-dd") ?? "");
                    xmlWriter.WriteElementString("Estado", p.PermissionStatusTypeId == (int)PermissionStatusEnum.Approved ? "Sí" : "No");
                    xmlWriter.WriteElementString("AuthorizedBy", p.AuthorizingUser?.Name ?? "");
                    xmlWriter.WriteElementString("CreatedAt", p.CreatedAt.ToString("yyyy-MM-dd HH:mm") ?? "");
                    xmlWriter.WriteEndElement(); 
                }
                xmlWriter.WriteEndElement(); 

                xmlWriter.WriteEndElement(); 
                xmlWriter.WriteEndDocument();

                string fileName = $"CorteTEC - ID {cutoffId}.xml";
                return (Encoding.UTF8.GetBytes(stringWriter.ToString()), fileName);
            }
        }
    }
}
