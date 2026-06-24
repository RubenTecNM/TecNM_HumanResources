using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace RecursosHumanosWeb.Services
{
    public class PermisoService : IPermisoService
    {
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PermisoService> _logger;

        public PermisoService(RecursosHumanosContext context, IMapper mapper, ILogger<PermisoService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IPagedList<PermisosDTO>> GetPermisosPagedAsync(int userId, int? areaId, string nombreUsuario, DateTime? delDia, DateTime? alDia, bool? revisado, int page, int pageSize)
        {
            var user = await _context.Usuarios.FindAsync(userId);
            if (user == null) return new List<PermisosDTO>().ToPagedList(1, pageSize);

            var query = _context.Permisos
                .Include(p => p.IdUsuarioSolicitaNavigation)
                .Include(p => p.IdTipoPermisoNavigation)
                .Where(p => p.IdEstadoPermiso != 3)
                .AsQueryable();

            // Restricciones por tipo de usuario (Seguridad y Reglas de Negocio)
            if (user.IdTipoUsuario == (int)TipoUsuarioEnum.Administrador || user.IdTipoUsuario == (int)TipoUsuarioEnum.RecursosHumanos)
            {
                // Full visibility: ven todo lo de todos los cortes
            }
            else if (user.IdTipoUsuario == (int)TipoUsuarioEnum.Autorizador)
            {
                var currentCourtId = await _context.Cortes
                    .Where(c => c.IdEstadoCorte == (int)CorteEstadoEnum.Abierto)
                    .OrderByDescending(c => c.Id)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();

                // Regla: Propios de cualquier corte O (De su área EN corte activo Y que aún no estén revisados)
                query = query.Where(p => p.IdUsuarioSolicita == userId || 
                                       (p.IdUsuarioSolicitaNavigation.IdArea == user.IdArea && p.IdCorte == currentCourtId && p.IdEstadoPermiso == (int)PermisoEstadoEnum.Pendiente));
            }
            else
            {
                // Solicitantes normales solo ven sus propios permisos (historial completo)
                query = query.Where(p => p.IdUsuarioSolicita == userId);
            }

            // Filtros manuales del panel de búsqueda
            if (revisado.HasValue)
            {
                if (revisado.Value)
                    query = query.Where(p => p.IdEstadoPermiso != (int)PermisoEstadoEnum.Pendiente);
                else
                    query = query.Where(p => p.IdEstadoPermiso == (int)PermisoEstadoEnum.Pendiente);
            }
            if (!string.IsNullOrEmpty(nombreUsuario))
                query = query.Where(p => p.IdUsuarioSolicitaNavigation.Nombre.Contains(nombreUsuario));

            if (areaId.HasValue)
                query = query.Where(p => p.IdUsuarioSolicitaNavigation.IdArea == areaId.Value);

            if (delDia.HasValue || alDia.HasValue)
            {
                // Un permiso debe aparecer si su periodo (Fecha1 a Fecha2) se solapa con el rango buscado.
                // Usamos lógica de "Touch/Overlap" para capturar permisos que iniciaron antes pero terminan dentro o después del rango.
                var startDate = delDia ?? new DateTime(1900, 1, 1);
                var endDate = alDia?.Date.AddDays(1).AddTicks(-1) ?? new DateTime(2100, 12, 31);
                
                query = query.Where(p => p.Fecha1 <= endDate && (p.Fecha2 == null || p.Fecha2 >= startDate));
            }

            return query
                .ProjectTo<PermisosDTO>(_mapper.ConfigurationProvider)
                .OrderByDescending(p => p.FechaCreacion)
                .ToPagedList(page, pageSize);
        }

        public async Task<object> GetStatsAsync(int userId, int userType)
        {
            IQueryable<Permiso> query = _context.Permisos.Where(p => p.IdEstadoPermiso != 3);

            if (userType != (int)TipoUsuarioEnum.Administrador && userType != (int)TipoUsuarioEnum.RecursosHumanos)
            {
                query = query.Where(p => p.IdUsuarioSolicita == userId);
            }

            var stats = await query
              .GroupBy(p => 1)
              .Select(g => new
              {
                  Total = g.Count(),
                  Pendientes = g.Count(p => p.IdEstadoPermiso == (int)PermisoEstadoEnum.Pendiente),
                  Aprobados = g.Count(p => p.IdEstadoPermiso == (int)PermisoEstadoEnum.Aprobado),
                  ConGoce = g.Count(p => p.Goce),
                  SinGoce = g.Count(p => !p.Goce),
                  EsteMes = g.Count(p => p.FechaCreacion.Month == DateTime.Now.Month && p.FechaCreacion.Year == DateTime.Now.Year)
              })
              .FirstOrDefaultAsync();

            return stats ?? new { Total = 0, Pendientes = 0, Aprobados = 0, ConGoce = 0, SinGoce = 0, EsteMes = 0 };
        }

        public async Task<int> CreatePermisoAsync(Permiso permiso, int idUsuarioCrea)
        {
            var userCrea = await _context.Usuarios.FindAsync(idUsuarioCrea);
            if (userCrea?.IdTipoUsuario == (int)TipoUsuarioEnum.Autorizador)
                throw new InvalidOperationException("Los jefes de área no pueden crear solicitudes de permiso para asegurar la imparcialidad del proceso.");

            var corteActivo = await _context.Cortes
                .Where(c => c.IdEstadoCorte == (int)CorteEstadoEnum.Abierto)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (corteActivo == null) throw new InvalidOperationException("No se encontró un corte activo.");

            // Validación de fechas contra el periodo del corte
            if (permiso.Fecha1 < corteActivo.Inicia || (permiso.Fecha2.HasValue && permiso.Fecha2 > corteActivo.Termina))
            {
                // REGLA DE NEGOCIO (Auto-Revisado sin goce)
                // Si excede el límite del corte actual, se aprueba automáticamente pero sin goce de sueldo
                permiso.IdEstadoPermiso = (int)PermisoEstadoEnum.Aprobado;
                permiso.Goce = false;
                // El autorizador por sistema puede dejarse nulo o registrar el ID sistema/creador
                permiso.FechaAutorizacion = DateTime.Now;
            }
            else
            {
                // Flujo normal para fechas dentro del corte
                permiso.IdEstadoPermiso = (int)PermisoEstadoEnum.Pendiente;
                // 'Goce' y otras banderas asumen los valores solicitados
            }

            permiso.IdCorte = corteActivo.Id;
            permiso.IdUsuarioCrea = idUsuarioCrea;
            permiso.IdUsuarioModifica = idUsuarioCrea;
            permiso.FechaCreacion = DateTime.Now;
            permiso.FechaModificacion = DateTime.Now;

            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();
            return permiso.Id;
        }

        public async Task<bool> UpdatePermisoAsync(Permiso permiso, int idUsuarioModifica)
        {
            var userModifica = await _context.Usuarios.FindAsync(idUsuarioModifica);
            if (userModifica?.IdTipoUsuario == (int)TipoUsuarioEnum.Autorizador)
                return false; // Jefes de área no pueden editar según reglas de negocio

            var existing = await _context.Permisos
                .Include(p => p.IdCorteNavigation)
                .FirstOrDefaultAsync(p => p.Id == permiso.Id);

            if (existing == null) return false;
            
            // Reglas de negocio
            if (existing.IdEstadoPermiso != (int)PermisoEstadoEnum.Pendiente || (existing.IdCorteNavigation != null && existing.IdCorteNavigation.IdEstadoCorte != (int)CorteEstadoEnum.Abierto))
                return false;

            int originalCorteId = existing.IdCorte;
            
            // Actualización explícita de campos editables
            existing.Motivo = permiso.Motivo;
            existing.Fecha1 = permiso.Fecha1;
            existing.Fecha2 = permiso.Fecha2;
            existing.IdTipoPermiso = permiso.IdTipoPermiso;
            existing.Evidencia = permiso.Evidencia;
            existing.Goce = permiso.Goce;
            
            existing.IdCorte = originalCorteId; // Preservar el periodo original
            existing.IdUsuarioModifica = idUsuarioModifica;
            existing.FechaModificacion = DateTime.Now;
            
            // Re-evaluar regla de negocio si está fuera del corte
            if (existing.IdCorteNavigation != null && (existing.Fecha1 < existing.IdCorteNavigation.Inicia || (existing.Fecha2.HasValue && existing.Fecha2 > existing.IdCorteNavigation.Termina)))
            {
                existing.IdEstadoPermiso = (int)PermisoEstadoEnum.Aprobado;
                existing.Goce = false;
                existing.FechaAutorizacion = DateTime.Now;
            }
            else
            {
                existing.IdEstadoPermiso = (int)PermisoEstadoEnum.Pendiente; // Reset revision on update si está dentro del corte
            }

            _context.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePermisoAsync(int id, int idUsuarioModifica)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null) return false;

            permiso.IdEstadoPermiso = (int)PermisoEstadoEnum.InvalidadoPorPeriodo;
            permiso.IdUsuarioModifica = idUsuarioModifica;
            permiso.FechaModificacion = DateTime.Now;

            _context.Update(permiso);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AutorizarPermisoAsync(int id, int idUsuarioAutoriza, bool aprobado, string comentario)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null) return false;

            permiso.IdEstadoPermiso = aprobado ? (int)PermisoEstadoEnum.Aprobado : (int)PermisoEstadoEnum.Rechazado;
            permiso.Goce = aprobado;
            permiso.IdUsuarioAutoriza = idUsuarioAutoriza;
            permiso.FechaAutorizacion = DateTime.Now;
            permiso.IdUsuarioModifica = idUsuarioAutoriza;
            permiso.FechaModificacion = DateTime.Now;

            _context.Update(permiso);
            return await _context.SaveChangesAsync() > 0;
        }

        public bool CanUserAccessPermiso(int userId, int userType, Permiso permiso)
        {
            if (userType == (int)TipoUsuarioEnum.Administrador || userType == (int)TipoUsuarioEnum.RecursosHumanos)
                return true;

            if (userId == permiso.IdUsuarioSolicita)
                return true;

            if (userType == (int)TipoUsuarioEnum.Autorizador)
            {
                var userArea = _context.Usuarios.AsNoTracking().Where(u => u.Id == userId).Select(u => u.IdArea).FirstOrDefault();
                if (permiso.IdUsuarioSolicitaNavigation != null && permiso.IdUsuarioSolicitaNavigation.IdArea == userArea)
                    return true;
                
                // If navigation not loaded, we might need to fetch it
                var solicitanteArea = _context.Usuarios.AsNoTracking().Where(u => u.Id == permiso.IdUsuarioSolicita).Select(u => u.IdArea).FirstOrDefault();
                return solicitanteArea == userArea;
            }
            
            return false;
        }
    }
}
