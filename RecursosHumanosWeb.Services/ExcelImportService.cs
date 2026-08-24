using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Helpers;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.Enums;

namespace RecursosHumanosWeb.Services;

/// <summary>Resultado de fila para preview/importación.</summary>
public record SyncRowResult(int Row, string Status, string Message, string? Detail = null);

public class SyncPreview
{
    public string Kind { get; set; } = "";
    public int TotalRows { get; set; }
    public int NewCount => Rows.Count(r => r.Status == "nuevo");
    public int DuplicateCount => Rows.Count(r => r.Status == "duplicado");
    public int ErrorCount => Rows.Count(r => r.Status == "error");
    public List<SyncRowResult> Rows { get; set; } = new();
}

public class SyncImportSummary
{
    public int CreatedUsers { get; set; }
    public int PromotedAuthorizers { get; set; }
    public int CreatedCutoffs { get; set; }
    public int CreatedPermissions { get; set; }
    public int Duplicates { get; set; }
    public List<SyncRowResult> Errors { get; set; } = new();
}

/// <summary>
/// Importación masiva de usuarios, permisos y autorizadores desde Excel.
/// Los permisos entran como Aprobados (sin goce por defecto) dentro del periodo declarado;
/// los periodos faltantes se crean cerrados antes de insertar sus permisos.
/// </summary>
public class ExcelImportService
{
    private readonly HumanResourcesContext _context;

    // ponytail: generador de claves con Random no cripto; suficiente para claves temporales de 10 chars que RH comunica
    private static readonly Random Rnd = new();
    private const string PwdChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";

    public ExcelImportService(HumanResourcesContext context)
    {
        _context = context;
    }

    public static string GenerateTempPassword()
    {
        Span<char> pwd = stackalloc char[10];
        for (int i = 0; i < pwd.Length; i++) pwd[i] = PwdChars[Rnd.Next(PwdChars.Length)];
        return new string(pwd);
    }

    // ────────────────────────────────────────────────────────────
    // VALIDACIÓN (preview sin escritura)
    // ────────────────────────────────────────────────────────────

    public async Task<SyncPreview> ValidateAsync(string kind, Stream excelStream)
    {
        using var wb = new XLWorkbook(excelStream);
        return kind switch
        {
            "usuarios" => await ValidateUsers(wb),
            "autorizadores" => await ValidateAuthorizers(wb),
            "permisos" => await ValidatePermissions(wb),
            _ => throw new ArgumentException("Tipo de carga desconocido.")
        };
    }

    private async Task<SyncPreview> ValidateUsers(XLWorkbook wb)
    {
        var ws = RequireSheet(wb, "Usuarios");
        var preview = new SyncPreview { Kind = "usuarios" };
        var emailsInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dbEmails = (await _context.Users.Select(u => u.Email).ToListAsync())
            .Select(e => e.ToLowerInvariant()).ToHashSet();
        var dbEmailSet = dbEmails.ToHashSet();

        var (areas, departments, positions, userTypes) = await LoadCatalogsAsync();

        foreach (var row in ReadRows(ws))
        {
            preview.TotalRows++;
            var email = Cell(row, "Correo");
            if (string.IsNullOrWhiteSpace(Cell(row, "Nombre")) || string.IsNullOrWhiteSpace(email))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", "Nombre y Correo son obligatorios."));
                continue;
            }
            var key = email.Trim().ToLowerInvariant();
            if (!emailsInFile.Add(key))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", "Correo duplicado dentro del archivo."));
                continue;
            }

            var areaErr = ResolveName(areas, Cell(row, "Área"), "Área", out _)
                          ?? ResolveName(departments, Cell(row, "Departamento"), "Departamento", out _)
                          ?? ResolveName(positions, Cell(row, "Puesto"), "Puesto", out _);
            var typeErr = ResolveUserType(userTypes, Cell(row, "Tipo"), out _, out var typeName);
            if (areaErr != null || typeErr != null)
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", areaErr ?? typeErr!));
                continue;
            }

            if (dbEmailSet.Contains(key))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "duplicado", $"Ya existe en la base de datos ({email})."));
            }
            else
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "nuevo", $"Se creará {Cell(row, "Nombre")} ({typeName}).", email));
            }
        }
        return preview;
    }

    private async Task<SyncPreview> ValidateAuthorizers(XLWorkbook wb)
    {
        var ws = RequireSheet(wb, "Autorizadores");
        var preview = new SyncPreview { Kind = "autorizadores" };
        var dbEmails = (await _context.Users.Select(u => u.Email).ToListAsync()).Select(e => e.ToLowerInvariant()).ToHashSet();
        var (areas, departments, positions, _) = await LoadCatalogsAsync();

        foreach (var row in ReadRows(ws))
        {
            preview.TotalRows++;
            var email = Cell(row, "Correo");
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(Cell(row, "Nombre")))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", "Nombre y Correo son obligatorios."));
                continue;
            }
            var err = ResolveName(areas, Cell(row, "Área"), "Área", out _)
                      ?? ResolveName(departments, Cell(row, "Departamento"), "Departamento", out _)
                      ?? ResolveName(positions, Cell(row, "Puesto"), "Puesto", out _);
            if (err != null)
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", err));
                continue;
            }

            if (dbEmails.Contains(email.Trim().ToLower()))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "duplicado", "Ya existe: será promovido a Autorizador y su área actualizada."));
            }
            else
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "nuevo", $"Se creará como Autorizador: {Cell(row, "Nombre")}.", email));
            }
        }
        return preview;
    }

    private async Task<SyncPreview> ValidatePermissions(XLWorkbook wb)
    {
        var wsPeriods = RequireSheet(wb, "Periodos");
        var wsPerms = RequireSheet(wb, "Permisos");
        var preview = new SyncPreview { Kind = "permisos" };

        var cutoffKeys = (await _context.Cutoffs.ToListAsync())
            .Select(c => (Start: c.StartDate.Date, End: c.EndDate.Date)).ToHashSet();
        foreach (var row in ReadRows(wsPeriods))
        {
            preview.TotalRows++;
            var ini = GetDate(row, "Fecha Inicio");
            var fin = GetDate(row, "Fecha Fin");
            if (!ini.HasValue || !fin.HasValue || ini > fin)
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", "Fechas de periodo inválidas."));
                continue;
            }
            if (!cutoffKeys.Add((ini.Value.Date, fin.Value.Date)))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "duplicado", "El periodo ya existe en la base de datos."));
            }
            else
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "nuevo", $"Se creará el periodo {ini:dd/MM/yyyy} – {fin:dd/MM/yyyy} (cerrado)."));
            }
        }

        var permTypes = await _context.PermissionTypes.ToDictionaryAsync(t => t.Description.Trim().ToLower(), t => t.Id);
        var userEmails = (await _context.Users.Where(u => u.Status == true)
            .Select(u => new { u.Id, u.Email }).ToListAsync())
            .GroupBy(u => u.Email.ToLower()).ToDictionary(g => g.Key, g => g.First().Id);
        var existingPerms = (await _context.Permissions
            .Select(p => new { p.RequestingUserId, p.PermissionTypeId, Start = p.StartDate!.Value.Date })
            .ToListAsync())
            .Select(x => (x.RequestingUserId, x.PermissionTypeId, x.Start)).ToHashSet();

        foreach (var row in ReadRows(wsPerms))
        {
            preview.TotalRows++;
            var email = Cell(row, "Correo Solicitante")?.Trim().ToLower();
            var typeName = Cell(row, "Tipo Permiso")?.Trim();
            var start = GetDate(row, "Fecha Inicio");

            if (string.IsNullOrEmpty(email) || !userEmails.TryGetValue(email, out var requesterId))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", "El solicitante no existe en la base de datos (cárguelo primero en Usuarios)."));
                continue;
            }
            if (string.IsNullOrEmpty(typeName) || !permTypes.TryGetValue(typeName.ToLower(), out var typeId))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", $"Tipo de permiso '{typeName}' no está en el catálogo."));
                continue;
            }
            if (!start.HasValue)
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", "La fecha de inicio es obligatoria."));
                continue;
            }

            // Reglas por tipo (misma lógica que el formulario): Falta Docente exige DOCENCIA y horas válidas
            if (typeName.Equals("Falta Docente", StringComparison.OrdinalIgnoreCase))
            {
                var docenciaId = await _context.Departments.AsNoTracking()
                    .Where(d => d.Description == "DOCENCIA").Select(d => (int?)d.Id).FirstOrDefaultAsync();
                var dept = await _context.Users.Where(u => u.Id == requesterId).Select(u => u.DepartmentId).FirstAsync();
                if (docenciaId == null || dept != docenciaId)
                {
                    preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", "Falta Docente solo aplica al personal de DOCENCIA."));
                    continue;
                }
                var horasAula = GetIntOrNull(row, "Horas Aula");
                var horasDescarga = GetIntOrNull(row, "Horas Descarga");
                int total = (horasAula ?? 0) + (horasDescarga ?? 0);
                if (total == 0)
                {
                    preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", "Para Falta Docente indique Horas Aula y/o Horas Descarga (> 0)."));
                    continue;
                }
                if (total > 7)
                {
                    preview.Rows.Add(new SyncRowResult(row.RowNumber(), "error", $"Las horas totales ({total}) exceden la duración máxima del turno (7h)."));
                    continue;
                }
            }

            if (existingPerms.Contains((requesterId, typeId, start.Value.Date)))
            {
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "duplicado", "Permiso ya registrado para ese solicitante, tipo y fecha."));
            }
            else
            {
                existingPerms.Add((requesterId, typeId, start.Value.Date));
                preview.Rows.Add(new SyncRowResult(row.RowNumber(), "nuevo", $"Se creará permiso '{typeName}' del {start:dd/MM/yyyy}."));
            }
        }
        return preview;
    }

    // ────────────────────────────────────────────────────────────
    // IMPORTACIÓN (escritura)
    // ────────────────────────────────────────────────────────────

    public async Task<SyncImportSummary> ImportAsync(string kind, Stream excelStream, int actorUserId)
    {
        using var wb = new XLWorkbook(excelStream);
        return kind switch
        {
            "usuarios" => await ImportUsers(wb, actorUserId),
            "autorizadores" => await ImportAuthorizers(wb, actorUserId),
            "permisos" => await ImportPermissions(wb, actorUserId),
            _ => throw new ArgumentException("Tipo de carga desconocido.")
        };
    }

    private async Task<SyncImportSummary> ImportUsers(XLWorkbook wb, int actorUserId)
    {
        var summary = new SyncImportSummary();
        var ws = RequireSheet(wb, "Usuarios");
        var (areas, departments, positions, userTypes) = await LoadCatalogsAsync();

        foreach (var row in ReadRows(ws))
        {
            var email = Cell(row, "Correo");
            if (string.IsNullOrWhiteSpace(Cell(row, "Nombre")) || string.IsNullOrWhiteSpace(email)) continue;
            var key = email.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == key)) { summary.Duplicates++; continue; }

            ResolveName(areas, Cell(row, "Área"), "Área", out var areaId);
            ResolveName(departments, Cell(row, "Departamento"), "Departamento", out var deptId);
            ResolveName(positions, Cell(row, "Puesto"), "Puesto", out var posId);
            ResolveUserType(userTypes, Cell(row, "Tipo"), out var typeId, out _);
            var err = (areaId == 0 ? $"El Área '{Cell(row, "Área")}' no existe en el catálogo." : null)
                      ?? (deptId == 0 ? $"El Departamento '{Cell(row, "Departamento")}' no existe en el catálogo." : null)
                      ?? (posId == 0 ? $"El Puesto '{Cell(row, "Puesto")}' no existe en el catálogo." : null)
                      ?? (typeId == 0 ? $"El Tipo '{Cell(row, "Tipo")}' no es válido (Trabajador, Autorizador, HumRes, Administrador)." : null);
            if (err != null)
            {
                summary.Errors.Add(new SyncRowResult(row.RowNumber(), "error", err));
                continue;
            }

            var tempPwd = GenerateTempPassword();
            _context.Users.Add(new User
            {
                Name = Cell(row, "Nombre").Trim(),
                Email = email.Trim(),
                UserTypeId = typeId,
                AreaId = areaId,
                DepartmentId = deptId,
                PositionId = posId,
                PasswordHash = PasswordHasher.HashSha256(tempPwd),
                TempPassword = tempPwd,
                Status = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedByUserId = actorUserId,
                UpdatedByUserId = actorUserId
            });
            summary.CreatedUsers++;
        }
        await _context.SaveChangesAsync();
        return summary;
    }

    private async Task<SyncImportSummary> ImportAuthorizers(XLWorkbook wb, int actorUserId)
    {
        var summary = new SyncImportSummary();
        var ws = RequireSheet(wb, "Autorizadores");
        var (areas, departments, positions, _) = await LoadCatalogsAsync();

        foreach (var row in ReadRows(ws))
        {
            var email = Cell(row, "Correo");
            if (string.IsNullOrWhiteSpace(email)) continue;
            var key = email.Trim().ToLower();
            ResolveName(areas, Cell(row, "Área"), "Área", out var areaId);
            ResolveName(departments, Cell(row, "Departamento"), "Departamento", out var deptId);
            ResolveName(positions, Cell(row, "Puesto"), "Puesto", out var posId);
            var err = (areaId == 0 ? $"El Área '{Cell(row, "Área")}' no existe en el catálogo." : null)
                      ?? (deptId == 0 ? $"El Departamento '{Cell(row, "Departamento")}' no existe en el catálogo." : null)
                      ?? (posId == 0 ? $"El Puesto '{Cell(row, "Puesto")}' no existe en el catálogo." : null);
            if (err != null)
            {
                summary.Errors.Add(new SyncRowResult(row.RowNumber(), "error", err));
                continue;
            }

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == key);
            if (existing != null)
            {
                if (existing.UserTypeId != (int)UserTypeEnum.Authorizer)
                {
                    existing.UserTypeId = (int)UserTypeEnum.Authorizer;
                    summary.PromotedAuthorizers++;
                }
                existing.AreaId = areaId;
                existing.UpdatedByUserId = actorUserId;
                existing.UpdatedAt = DateTime.Now;
                continue;
            }

            if (string.IsNullOrWhiteSpace(Cell(row, "Nombre"))) continue;
            var tempPwd = GenerateTempPassword();
            _context.Users.Add(new User
            {
                Name = Cell(row, "Nombre").Trim(),
                Email = email.Trim(),
                UserTypeId = (int)UserTypeEnum.Authorizer,
                AreaId = areaId,
                DepartmentId = deptId,
                PositionId = posId,
                PasswordHash = PasswordHasher.HashSha256(tempPwd),
                TempPassword = tempPwd,
                Status = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedByUserId = actorUserId,
                UpdatedByUserId = actorUserId
            });
            summary.CreatedUsers++;
        }
        await _context.SaveChangesAsync();
        return summary;
    }

    private async Task<SyncImportSummary> ImportPermissions(XLWorkbook wb, int actorUserId)
    {
        var summary = new SyncImportSummary();
        var wsPeriods = RequireSheet(wb, "Periodos");

        // 1. Periodos: crear los faltantes como CERRADOS antes que sus permisos
        var cutoffs = await _context.Cutoffs.ToListAsync();
        var byKey = cutoffs.ToDictionary(c => (Start: c.StartDate.Date, End: c.EndDate.Date));
        foreach (var row in ReadRows(wsPeriods))
        {
            var ini = GetDate(row, "Fecha Inicio");
            var fin = GetDate(row, "Fecha Fin");
            if (!ini.HasValue || !fin.HasValue || ini > fin) continue;
            var key = (ini.Value.Date, fin.Value.Date);
            if (byKey.ContainsKey(key)) { summary.Duplicates++; continue; }
            var cutoff = new Cutoff
            {
                StartDate = ini.Value.Date,
                EndDate = fin.Value.Date,
                CutoffStatusTypeId = (int)CutoffStatusEnum.Closed,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedByUserId = actorUserId,
                UpdatedByUserId = actorUserId
            };
            _context.Cutoffs.Add(cutoff);
            byKey[key] = cutoff;
            summary.CreatedCutoffs++;
        }
        await _context.SaveChangesAsync(); // asigna Id a los nuevos periodos

        // 2. Permisos
        var wsPerms = RequireSheet(wb, "Permisos");
        var users = await _context.Users.ToDictionaryAsync(u => u.Email.ToLower(), u => u);
        var types = await _context.PermissionTypes.ToDictionaryAsync(t => t.Description.Trim().ToLower(), t => t);

        foreach (var row in ReadRows(wsPerms))
        {
            var email = Cell(row, "Correo Solicitante")?.Trim().ToLower() ?? "";
            var typeName = Cell(row, "Tipo Permiso")?.Trim().ToLower() ?? "";
            var start = GetDate(row, "Fecha Inicio");

            if (!users.TryGetValue(email, out var requester)
                || !types.TryGetValue(typeName, out var ptype)
                || !start.HasValue) continue;

            var end = GetDate(row, "Fecha Fin");
            var periodIni = GetDate(row, "Periodo Inicio");
            var periodFin = GetDate(row, "Periodo Fin");
            var cutoff = periodIni.HasValue && periodFin.HasValue && byKey.TryGetValue((periodIni.Value.Date, periodFin.Value.Date), out var c)
                ? c : byKey.Values.OrderBy(x => Math.Abs((x.StartDate.Date - start.Value.Date).Ticks)).First();

            var dupKey = (RequestingUserId: requester.Id, TypeId: ptype.Id, Start: start.Value.Date);
            bool dup = await _context.Permissions.AnyAsync(p =>
                p.RequestingUserId == dupKey.RequestingUserId && p.PermissionTypeId == dupKey.TypeId &&
                p.StartDate.HasValue && p.StartDate.Value.Date == dupKey.Start);
            if (dup) { summary.Duplicates++; continue; }

            bool withPay = ParseBoolOrDefault(Cell(row, "Con Goce"), defaultValue: false);

            var perm = new Permission
            {
                RequestingUserId = requester.Id,
                PermissionTypeId = ptype.Id,
                StartDate = start,
                EndDate = end ?? start,
                Reason = string.IsNullOrWhiteSpace(Cell(row, "Motivo")) ? "Carga masiva" : Cell(row, "Motivo").Trim(),
                WithPay = withPay,
                CutoffId = cutoff.Id,
                PermissionStatusTypeId = (int)PermissionStatusEnum.Approved,
                AuthorizedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedByUserId = actorUserId,
                UpdatedByUserId = actorUserId
            };

            bool esFaltaDocente = ptype.Description.Trim().Equals("Falta Docente", StringComparison.OrdinalIgnoreCase);
            if (esFaltaDocente) // Falta Docente: solo DOCENCIA + turno/horas válidas (misma regla del formulario)
            {
                var docenciaId = await _context.Departments.AsNoTracking()
                    .Where(d => d.Description == "DOCENCIA").Select(d => (int?)d.Id).FirstOrDefaultAsync();
                if (docenciaId == null || requester.DepartmentId != docenciaId)
                {
                    summary.Errors.Add(new SyncRowResult(row.RowNumber(), "error",
                        "El permiso por Falta Docente solo aplica al personal del departamento DOCENCIA."));
                    continue;
                }
                perm.EndDate = start;
                perm.Days = 1;
                perm.HoursInClassroom = GetIntOrNull(row, "Horas Aula");
                perm.HoursDischarge = GetIntOrNull(row, "Horas Descarga");
                int hour = start.Value.Hour;
                perm.Shift = hour >= 16 ? 2 : 1; // vespertino desde las 16; matutino en caso contrario
                int total = (perm.HoursInClassroom ?? 0) + (perm.HoursDischarge ?? 0);
                if (total == 0 || total > (perm.Shift == 1 ? 7 : 6))
                {
                    summary.Errors.Add(new SyncRowResult(row.RowNumber(), "error",
                        total == 0 ? "Horas totales deben ser mayores a 0." :
                        $"Las horas totales ({total}) exceden la duración del turno ({(perm.Shift == 1 ? 7 : 6)}h)."));
                    continue;
                }
            }
            else
            {
                perm.Days = ((end?.Date ?? start.Value.Date) - start.Value.Date).Days + 1;
            }

            _context.Permissions.Add(perm);
            summary.CreatedPermissions++;
        }
        await _context.SaveChangesAsync();
        return summary;
    }

    // ────────────────────────────────────────────────────────────
    // PLANTILLAS
    // ────────────────────────────────────────────────────────────

    public static byte[] BuildTemplate(string kind)
    {
        var wb = new XLWorkbook();
        switch (kind)
        {
            case "usuarios":
                AddSheet(wb, "Usuarios", new[]
                {
                    ("Nombre", "Juan Pérez López"),
                    ("Correo", "jperez@tecnm.mx"),
                    ("Tipo", "Trabajador"),
                    ("Área", "Dirección Académica"),
                    ("Departamento", "DOCENCIA"),
                    ("Puesto", "Docente")
                });
                AddLegend(wb, new[]
                {
                    ("Nombre", "Texto. Nombre completo del usuario."),
                    ("Correo", "Correo institucional ÚNICO. Es la llave de comparación: si ya existe en la base de datos NO se vuelve a crear."),
                    ("Tipo", "Trabajador | Autorizador | HumRes | Administrador. Vacío = Trabajador."),
                    ("Área", "Nombre exacto del catálogo de Áreas."),
                    ("Departamento", "Nombre exacto del catálogo de Departamentos."),
                    ("Puesto", "Nombre exacto del catálogo de Puestos.")
                }, "Los usuarios nuevos reciben una clave temporal aleatoria visible para RH en la vista de Usuarios hasta que el usuario ingresa al sistema.");
                break;

            case "autorizadores":
                AddSheet(wb, "Autorizadores", new[]
                {
                    ("Nombre", "María González Ruiz"),
                    ("Correo", "mgonzalez@tecnm.mx"),
                    ("Área", "Planeación y Vinculación"),
                    ("Departamento", "Recursos Humanos"),
                    ("Puesto", "Jefa de Departamento")
                });
                AddLegend(wb, new[]
                {
                    ("Nombre", "Texto. Obligatorio solo si el usuario aún no existe."),
                    ("Correo", "Llave de comparación. Si existe → se promueve a Autorizador y se actualiza su Área. Si no existe → se crea nuevo usuario tipo Autorizador."),
                    ("Área", "Nombre exacto del catálogo. Será el área cuyas solicitudes autorizará."),
                    ("Departamento", "Nombre exacto del catálogo (obligatorio si es nuevo)."),
                    ("Puesto", "Nombre exacto del catálogo (obligatorio si es nuevo).")
                }, "Los autorizadores no pueden crear ni editar solicitudes (imparcialidad); solo autorizan las de su área.");
                break;

            case "permisos":
                AddSheet(wb, "Periodos", new[]
                {
                    ("Fecha Inicio", "01/05/2026"),
                    ("Fecha Fin", "30/06/2026")
                });
                AddSheet(wb, "Permisos", new[]
                {
                    ("Correo Solicitante", "jperez@tecnm.mx"),
                    ("Tipo Permiso", "Falta Docente"),
                    ("Periodo Inicio", "01/05/2026"),
                    ("Periodo Fin", "30/06/2026"),
                    ("Fecha Inicio", "15/05/2026 08:00"),
                    ("Fecha Fin", "(vacío = mismo día)"),
                    ("Con Goce", "No"),
                    ("Motivo", "Asuntos particulares"),
                    ("Horas Aula", "4"),
                    ("Horas Descarga", "2")
                });
                AddLegend(wb, new[]
                {
                    ("Hoja Periodos", "Fechas dd/mm/aaaa. Si el periodo no existe se crea como CERRADO antes de cargar sus permisos."),
                    ("Correo Solicitante", "Debe existir previamente en Usuarios (cargue primero esa plantilla)."),
                    ("Tipo Permiso", "Nombre exacto del catálogo, p.ej. 'Falta Docente'."),
                    ("Periodo Inicio / Periodo Fin", "Deben coincidir con una fila de la hoja Periodos."),
                    ("Fecha Inicio", "dd/mm/aaaa [hh:mm]. La hora define turno para Falta Docente (<14:00 matutino, ≥16:00 vespertino)."),
                    ("Con Goce", "Sí/No o 1/0. Vacío = No (aprobado sin goce). Todos los importados entran como Aprobados."),
                    ("Horas Aula / Horas Descarga", "Solo Falta Docente. Suma entre 1 y 7 (matutino) o 6 (vespertino).")
                }, "Duplicado = mismo solicitante + mismo tipo + misma fecha de inicio. Esos registros se omiten.");
                break;

            default:
                throw new ArgumentException("Tipo de carga desconocido.");
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ────────────────────────────────────────────────────────────
    // HELPERS
    // ────────────────────────────────────────────────────────────

    private static IXLWorksheet RequireSheet(XLWorkbook wb, string name)
    {
        var ws = wb.Worksheet(name);
        if (ws == null) throw new InvalidOperationException($"El archivo no contiene la hoja requerida '{name}'.");
        return ws;
    }

    private sealed class RowReader
    {
        public IXLRangeRow Row { get; init; }
        public Dictionary<string, int> Col { get; init; }
        public int RowNumber() => Row.RowNumber();
    }

    private static IEnumerable<RowReader> ReadRows(IXLWorksheet ws)
    {
        var used = ws.RangeUsed();
        if (used == null) yield break;
        var header = used.FirstRow();
        if (header == null) yield break;

        var col = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in header.CellsUsed())
            col[cell.GetString().Trim()] = cell.Address.ColumnNumber;

        foreach (var r in used.RowsUsed().Skip(1))
        {
            // saltar filas totalmente vacías
            if (!r.CellsUsed().Any()) continue;
            yield return new RowReader { Row = r, Col = col };
        }
    }

    private static string? Cell(RowReader row, string column)
    {
        if (!row.Col.TryGetValue(column, out int n)) return null;
        var v = row.Row.Cell(n).GetString();
        return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }

    private static DateTime? GetDate(RowReader row, string column)
    {
        var raw = Cell(row, column);
        if (raw == null) return null;
        if (row.Col.TryGetValue(column, out int n))
        {
            var cell = row.Row.Cell(n);
            try { if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime(); } catch { }
        }
        string[] formats = { "dd/MM/yyyy HH:mm", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy-MM-dd HH:mm" };
        if (DateTime.TryParseExact(raw, formats, new CultureInfo("es-MX"), DateTimeStyles.None, out var dt)) return dt;
        return DateTime.TryParse(raw, new CultureInfo("es-MX"), DateTimeStyles.None, out dt) ? dt : null;
    }

    private static int? GetIntOrNull(RowReader row, string column)
    {
        var raw = Cell(row, column);
        return int.TryParse(raw, out var i) ? i : null;
    }

    private static bool ParseBoolOrDefault(string? raw, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(raw)) return defaultValue;
        var v = raw.Trim().ToLowerInvariant();
        if (v is "si" or "sí" or "1" or "true" or "x") return true;
        if (v is "no" or "0" or "false") return false;
        return defaultValue;
    }

    private async Task<(List<Area>, List<Department>, List<Position>, List<UserType>)> LoadCatalogsAsync()
    {
        var areas = await _context.Areas.ToListAsync();
        var departments = await _context.Departments.ToListAsync();
        var positions = await _context.Positions.ToListAsync();
        var userTypes = await _context.UserTypes.ToListAsync();
        return (areas, departments, positions, userTypes);
    }

    private static string? ResolveName<T>(List<T> catalog, string? input, string label, out int id) where T : BaseEntity
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(input)) return $"{label} es obligatorio.";
        var match = catalog.FirstOrDefault(x =>
        {
            var prop = x.GetType().GetProperty("Description");
            return prop?.GetValue(x)?.ToString()?.Equals(input.Trim(), StringComparison.OrdinalIgnoreCase) == true;
        });
        if (match == null) return $"'{input}' no coincide con ningún elemento del catálogo de {label}.";
        id = match.Id;
        return null;
    }

    private static string? ResolveUserType(List<UserType> catalog, string? input, out int id, out string name)
    {
        id = 0; name = "";
        if (string.IsNullOrWhiteSpace(input)) { id = (int)UserTypeEnum.Employee; name = "Trabajador"; return null; }
        var clean = input.Trim();
        var match = catalog.FirstOrDefault(t => t.Description.Equals(clean, StringComparison.OrdinalIgnoreCase));
        if (match != null) { id = match.Id; name = match.Description; return null; }
        // Fallback por nombre estándar (catálogo incompleto)
        var byEnum = clean.ToLowerInvariant() switch
        {
            "trabajador" or "empleado" => UserTypeEnum.Employee,
            "autorizador" => UserTypeEnum.Authorizer,
            "humres" or "recursos humanos" => UserTypeEnum.HumanResources,
            "administrador" => UserTypeEnum.Administrator,
            _ => (UserTypeEnum?)null
        };
        if (byEnum.HasValue) { id = (int)byEnum.Value; name = clean; return null; }
        return $"'{input}' no es un tipo de usuario válido (Trabajador, Autorizador, HumRes, Administrador).";
    }

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

    private static void AddLegend(XLWorkbook wb, (string Column, string Description)[] entries, string note)
    {
        var ws = wb.Worksheets.Add("Leyenda");
        ws.Cell(1, 1).Value = "Columna / Concepto";
        ws.Cell(1, 2).Value = "Descripción y formato requerido";
        ws.Range(1, 1, 1, 2).Style.Font.Bold = true;
        ws.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1B396A");
        ws.Range(1, 1, 1, 2).Style.Font.FontColor = XLColor.White;
        for (int i = 0; i < entries.Length; i++)
        {
            ws.Cell(i + 2, 1).Value = entries[i].Column;
            ws.Cell(i + 2, 2).Value = entries[i].Description;
        }
        ws.Cell(entries.Length + 3, 1).Value = "Nota:";
        ws.Cell(entries.Length + 3, 1).Style.Font.Bold = true;
        ws.Cell(entries.Length + 3, 2).Value = note;
        ws.Column(1).AdjustToContents();
        ws.Column(2).AdjustToContents();
    }
}
