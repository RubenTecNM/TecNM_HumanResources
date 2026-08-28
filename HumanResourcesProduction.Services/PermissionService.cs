using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.DTOs;
using HumanResourcesProduction.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace HumanResourcesProduction.Services
{
    public class PermissionService : IPermissionService
    {
        private const string StatsVersionKey = "permission:stats:version";
        private const string StatsCachePrefix = "permission:stats:";
        private static readonly TimeSpan StatsCacheDuration = TimeSpan.FromSeconds(30);

        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PermissionService> _logger;
        private readonly IMemoryCache _cache;

        public PermissionService(HumanResourcesContext context, IMapper mapper, ILogger<PermissionService> logger, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IPagedList<PermissionsDTO>> GetPermissionsPagedAsync(int userId, int? areaId, string userName, DateTime? fromDay, DateTime? toDay, int? statusId, int page, int pageSize)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<PermissionsDTO>().ToPagedList(1, pageSize);

            var query = _context.Permissions
                .Include(p => p.RequestingUser)
                .Include(p => p.PermissionType)
                .Include(p => p.ShiftType)
                .AsQueryable();

            // Restricciones por tipo de user (Seguridad y Reglas de Negocio)
            if (user.UserTypeId == (int)UserTypeEnum.Administrator || user.UserTypeId == (int)UserTypeEnum.HumanResources)
            {
                // Full visibility: ven todo lo de todos los cutoffs
            }
            else if (user.UserTypeId == (int)UserTypeEnum.Authorizer)
            {
                var currentCutoffId = await _context.Cutoffs
                    .Where(c => c.CutoffStatusTypeId == (int)CutoffStatusEnum.Open)
                    .OrderByDescending(c => c.Id)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();

                // Regla: Propios de cualquier cutoff O (De su área EN cutoff activo Y que aún no estén revisados)
                query = query.Where(p => p.RequestingUserId == userId || 
                                       (p.RequestingUser.AreaId == user.AreaId && p.CutoffId == currentCutoffId && p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending));
            }
            else
            {
                // Solicitantes normales solo ven sus propios permissions (historial completo)
                query = query.Where(p => p.RequestingUserId == userId);
            }

            // Filtros manuales del panel de búsqueda
            if (statusId.HasValue)
                query = query.Where(p => p.PermissionStatusTypeId == statusId.Value);
            if (!string.IsNullOrEmpty(userName))
                query = query.Where(p => p.RequestingUser.Name.Contains(userName));

            if (areaId.HasValue)
                query = query.Where(p => p.RequestingUser.AreaId == areaId.Value);

            if (fromDay.HasValue || toDay.HasValue)
            {
                // Un permission debe aparecer si su periodo (StartDate a EndDate) se solapa con el rango buscado.
                // Usamos lógica de "Touch/Overlap" para capturar permissions que iniciaron antes pero terminan dentro o después del rango.
                var startDate = fromDay ?? new DateTime(1900, 1, 1);
                var endDate = toDay?.Date.AddDays(1).AddTicks(-1) ?? new DateTime(2100, 12, 31);
                
                query = query.Where(p => p.StartDate <= endDate && (p.EndDate == null || p.EndDate >= startDate));
            }

            return query
                .ProjectTo<PermissionsDTO>(_mapper.ConfigurationProvider)
                .OrderByDescending(p => p.CreatedAt)
                .ToPagedList(page, pageSize);
        }

        public async Task<object> GetStatsAsync(int userId, int userType)
        {
            var version = _cache.Get<int>(StatsVersionKey);
            var cacheKey = $"{StatsCachePrefix}{userType}:{userId}:v{version}";

            if (_cache.TryGetValue(cacheKey, out object? cached) && cached != null)
                return cached;

            IQueryable<Permission> query = _context.Permissions;

            if (userType != (int)UserTypeEnum.Administrator && userType != (int)UserTypeEnum.HumanResources)
            {
                query = query.Where(p => p.RequestingUserId == userId);
            }

            var stats = await query
              .GroupBy(p => 1)
              .Select(g => new
              {
                  Total = g.Count(),
                  Pending = g.Count(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending),
                  Approved = g.Count(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Approved),
                  WithPay = g.Count(p => p.WithPay),
                  WithoutPay = g.Count(p => !p.WithPay),
                  ThisMonth = g.Count(p => p.CreatedAt.Month == DateTime.Now.Month && p.CreatedAt.Year == DateTime.Now.Year)
              })
              .FirstOrDefaultAsync();

            var result = stats ?? new { Total = 0, Pending = 0, Approved = 0, WithPay = 0, WithoutPay = 0, ThisMonth = 0 };

            _cache.Set(cacheKey, result, StatsCacheDuration);

            return result;
        }

        public async Task<int> CreatePermissionAsync(Permission permission, int createdByUserId)
        {
            var createdBy = await _context.Users.FindAsync(createdByUserId);
            if (createdBy?.UserTypeId == (int)UserTypeEnum.Authorizer)
                throw new InvalidOperationException("Los jefes de área no pueden crear solicitudes de permiso para asegurar la imparcialidad del proceso.");

                // Teacher Absence validation
                if (permission.PermissionTypeId == 5)
                {
                    // Solo aplica al personal del departamento DOCENCIA, sin importar quién capture.
                    var requestingUser = await _context.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == permission.RequestingUserId);
                    var docenciaId = await _context.Departments.AsNoTracking()
                        .Where(d => d.Description == "DOCENCIA")
                        .Select(d => (int?)d.Id)
                        .FirstOrDefaultAsync();
                    if (requestingUser == null || docenciaId == null || requestingUser.DepartmentId != docenciaId)
                        throw new InvalidOperationException("El permiso por Falta Docente solo aplica al personal del departamento DOCENCIA.");

                // Auto-derive shift from StartDate hour:
                //   < 14  → Morning (1): 7am-2pm, max 7h
                //   >= 16 → Afternoon (2): 4pm-10pm, max 6h
                //   [14,16) → error (gap between shifts)
                if (!permission.StartDate.HasValue)
                    throw new InvalidOperationException("La fecha de inicio es obligatoria para falta de docente.");

                int hour = permission.StartDate.Value.Hour;
                if (hour >= 14 && hour < 16)
                    throw new InvalidOperationException("La hora ingresada está en el intervalo sin clases (14:00-16:00). Use turno Matutino (7am-2pm) o Vespertino (4pm-10pm).");

                permission.Shift = hour < 14 ? 1 : 2; // 1=Morning, 2=Afternoon

                int totalHours = (permission.HoursInClassroom ?? 0) + (permission.HoursDischarge ?? 0);
                if (totalHours == 0)
                    throw new InvalidOperationException("Las horas totales deben ser mayor a 0. Si no asistió, registre una falta.");

                int maxHours = permission.Shift == 1 ? 7 : 6; // Morning=7, Afternoon=6
                if (totalHours > maxHours)
                    throw new InvalidOperationException($"Las horas totales ({totalHours}) exceden la duración del turno ({maxHours}h).");

                // Single-day absence for teacher type
                permission.EndDate = permission.StartDate;
                permission.Days = 1;
            }

            var activeCutoff = await _context.Cutoffs
                .Where(c => c.CutoffStatusTypeId == (int)CutoffStatusEnum.Open)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (activeCutoff == null) throw new InvalidOperationException("Actualmente no hay un periodo activo para registrar permisos. Recursos Humanos está definiendo el nuevo corte; intenta de nuevo más tarde.");

            // Validación de fechas contra el periodo del cutoff
            if (permission.StartDate < activeCutoff.StartDate || (permission.EndDate.HasValue && permission.EndDate > activeCutoff.EndDate))
            {
                // REGLA DE NEGOCIO (Auto-Revisado sin goce)
                // Si excede el límite del cutoff actual, se aprueba automáticamente pero sin goce de sueldo
                permission.PermissionStatusTypeId = (int)PermissionStatusEnum.Approved;
                permission.WithPay = false;
                // El autorizador por sistema puede dejarse nulo o registrar el ID sistema/creador
                permission.AuthorizedAt = DateTime.Now;
            }
            else
            {
                // Flujo normal para fechas dentro del cutoff
                permission.PermissionStatusTypeId = (int)PermissionStatusEnum.Pending;
                // 'WithPay' y otras banderas asumen los valores solicitados
            }

            permission.CutoffId = activeCutoff.Id;
            permission.CreatedByUserId = createdByUserId;
            permission.UpdatedByUserId = createdByUserId;
            permission.CreatedAt = DateTime.Now;
            permission.UpdatedAt = DateTime.Now;

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            BumpStatsVersion();
            return permission.Id;
        }

        public async Task<bool> UpdatePermissionAsync(Permission permission, int updatedByUserId)
        {
            var updatedBy = await _context.Users.FindAsync(updatedByUserId);
            if (updatedBy?.UserTypeId == (int)UserTypeEnum.Authorizer)
                return false; // Jefes de área no pueden editar según reglas de negocio

            var existing = await _context.Permissions
                .Include(p => p.Cutoff)
                .FirstOrDefaultAsync(p => p.Id == permission.Id);

            if (existing == null) return false;
            
            // Reglas de negocio
            bool corteVigente = existing.Cutoff != null
                && existing.Cutoff.CutoffStatusTypeId == (int)CutoffStatusEnum.Open
                && existing.Cutoff.EndDate.Date >= DateTime.Now.Date;
            if (existing.PermissionStatusTypeId != (int)PermissionStatusEnum.Pending || !corteVigente)
                return false;

            int originalCutoffId = existing.CutoffId;
            
            // Actualización explícita de campos editables
            existing.Reason = permission.Reason;
            existing.StartDate = permission.StartDate;
            existing.EndDate = permission.EndDate;
            // El tipo de permiso NO es editable: se conserva el original (si se equivocó, crea/edita otro permiso)
            existing.PermissionTypeId = existing.PermissionTypeId;
            existing.Evidence = permission.Evidence;
            existing.WithPay = permission.WithPay;

            // Teacher Absence fields
            existing.HoursInClassroom = permission.HoursInClassroom;
            existing.HoursDischarge = permission.HoursDischarge;

            // For type 5, enforce single-day + auto-derive shift
            if (existing.PermissionTypeId == 5)
            {
                if (!existing.StartDate.HasValue)
                    throw new InvalidOperationException("La fecha de inicio es obligatoria para falta de docente.");

                int hour = existing.StartDate.Value.Hour;
                if (hour >= 14 && hour < 16)
                    throw new InvalidOperationException("La hora ingresada está en el intervalo sin clases (14:00-16:00). Use turno Matutino (7am-2pm) o Vespertino (4pm-10pm).");

                existing.Shift = hour < 14 ? 1 : 2;

                int totalHours = (existing.HoursInClassroom ?? 0) + (existing.HoursDischarge ?? 0);
                if (totalHours == 0)
                    throw new InvalidOperationException("Las horas totales deben ser mayor a 0. Si no asistió, registre una falta.");

                int maxHours = existing.Shift == 1 ? 7 : 6;
                if (totalHours > maxHours)
                    throw new InvalidOperationException($"Las horas totales ({totalHours}) exceden la duración del turno ({maxHours}h).");

                existing.EndDate = existing.StartDate;
                existing.Days = 1;
            }
            
            existing.CutoffId = originalCutoffId; // Preservar el periodo original
            existing.UpdatedByUserId = updatedByUserId;
            existing.UpdatedAt = DateTime.Now;
            
            // Re-evaluar regla de negocio si está fuera del cutoff
            if (existing.Cutoff != null && (existing.StartDate < existing.Cutoff.StartDate || (existing.EndDate.HasValue && existing.EndDate > existing.Cutoff.EndDate)))
            {
                existing.PermissionStatusTypeId = (int)PermissionStatusEnum.Approved;
                existing.WithPay = false;
                existing.AuthorizedAt = DateTime.Now;
            }
            else
            {
                existing.PermissionStatusTypeId = (int)PermissionStatusEnum.Pending; // Reset revision on update si está dentro del cutoff
            }

            _context.Update(existing);
            var saved = await _context.SaveChangesAsync() > 0;
            if (saved) BumpStatsVersion();
            return saved;
        }

        public async Task<bool> CancelPermissionAsync(int id, int updatedByUserId)
        {
            var permission = await _context.Permissions
                .Include(p => p.Cutoff)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (permission == null) return false;

            // Solo se cancela un permiso pendiente dentro de un corte vigente y no revisado
            bool corteVigente = permission.Cutoff != null
                && permission.Cutoff.CutoffStatusTypeId == (int)CutoffStatusEnum.Open
                && permission.Cutoff.EndDate.Date >= DateTime.Now.Date;
            if (permission.PermissionStatusTypeId != (int)PermissionStatusEnum.Pending || !corteVigente)
                return false;

            permission.PermissionStatusTypeId = (int)PermissionStatusEnum.Cancelled;
            permission.UpdatedByUserId = updatedByUserId;
            permission.UpdatedAt = DateTime.Now;

            _context.Update(permission);
            var saved = await _context.SaveChangesAsync() > 0;
            if (saved) BumpStatsVersion();
            return saved;
        }

        public async Task<bool> AuthorizePermissionAsync(int id, int authorizingUserId, bool aprobado, bool withPay, string? comment)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return false;

            permission.PermissionStatusTypeId = aprobado ? (int)PermissionStatusEnum.Approved : (int)PermissionStatusEnum.Rejected;
            permission.WithPay = aprobado && withPay;
            permission.AuthorizationComment = string.IsNullOrWhiteSpace(comment) ? null : comment?.Trim();
            permission.AuthorizingUserId = authorizingUserId;
            permission.AuthorizedAt = DateTime.Now;
            permission.UpdatedByUserId = authorizingUserId;
            permission.UpdatedAt = DateTime.Now;

            _context.Update(permission);
            var saved = await _context.SaveChangesAsync() > 0;
            if (saved) BumpStatsVersion();
            return saved;
        }

        public bool CanUserAccessPermiso(int userId, int userType, Permission permission)
        {
            if (userType == (int)UserTypeEnum.Administrator || userType == (int)UserTypeEnum.HumanResources)
                return true;

            if (userId == permission.RequestingUserId)
                return true;

            if (userType == (int)UserTypeEnum.Authorizer)
            {
                // Un permiso cancelado solo es visible para su solicitante, RH y administrador.
                if (permission.PermissionStatusTypeId == (int)PermissionStatusEnum.Cancelled)
                    return false;

                var userArea = _context.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.AreaId).FirstOrDefault();
                if (permission.RequestingUser != null && permission.RequestingUser.AreaId == userArea)
                    return true;
                
                // If navigation not loaded, we might need to fetch it
                var requesterArea = _context.Users.AsNoTracking().Where(u => u.Id == permission.RequestingUserId).Select(u => u.AreaId).FirstOrDefault();
                return requesterArea == userArea;
            }
            
            return false;
        }

        private void BumpStatsVersion()
        {
            _cache.Set(StatsVersionKey, _cache.Get<int>(StatsVersionKey) + 1, StatsCacheDuration);
        }
    }
}
