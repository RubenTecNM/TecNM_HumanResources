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

        public async Task<IPagedList<PermisosDTO>> GetPermisosPagedAsync(int userId, int? areaId, string nombreUsuario, DateTime? delDia, DateTime? alDia, int page, int pageSize)
        {
            var user = await _context.Usuarios.FindAsync(userId);
            if (user == null) return new List<PermisosDTO>().ToPagedList(1, pageSize);

            var query = _context.Permisos
                .Include(p => p.IdUsuarioSolicitaNavigation)
                .Include(p => p.IdTipoPermisoNavigation)
                .Where(p => p.Estatus)
                .AsQueryable();

            // Restricciones por tipo de usuario
            if (user.IdTipoUsuario == (int)TipoUsuarioEnum.Administrador || user.IdTipoUsuario == (int)TipoUsuarioEnum.RecursosHumanos)
            {
                // Full visibility
            }
            else if (user.IdTipoUsuario == (int)TipoUsuarioEnum.Autorizador)
            {
                var currentCourtId = await _context.Cortes
                    .Where(c => c.Estatus)
                    .OrderByDescending(c => c.Id)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();

                query = query.Where(p => p.IdUsuarioSolicita == userId || 
                                       (p.IdUsuarioSolicitaNavigation.IdArea == user.IdArea && p.IdCorte == currentCourtId));
            }
            else
            {
                query = query.Where(p => p.IdUsuarioSolicita == userId);
            }

            // Filtros
            if (!string.IsNullOrEmpty(nombreUsuario))
                query = query.Where(p => p.IdUsuarioSolicitaNavigation.Nombre.Contains(nombreUsuario));

            if (areaId.HasValue)
                query = query.Where(p => p.IdUsuarioSolicitaNavigation.IdArea == areaId.Value);

            if (delDia.HasValue)
                query = query.Where(p => p.Fecha1 >= delDia.Value);

            if (alDia.HasValue)
            {
                // Un permiso debe aparecer si su periodo (Fecha1 a Fecha2) se solapa con el rango buscado.
                // Si solo se da 'alDia', buscamos permisos que hayan iniciado antes o en esa fecha 
                // Y que no hayan terminado antes de la fecha de inicio del filtro (si existe).
                var startDate = delDia ?? new DateTime(1900, 1, 1);
                query = query.Where(p => p.Fecha1 <= alDia.Value && (p.Fecha2 == null || p.Fecha2 >= startDate));
            }

            return query
                .ProjectTo<PermisosDTO>(_mapper.ConfigurationProvider)
                .OrderByDescending(p => p.FechaCreacion)
                .ToPagedList(page, pageSize);
        }

        public async Task<object> GetStatsAsync(int userId, int userType)
        {
            IQueryable<Permiso> query = _context.Permisos.Where(p => p.Estatus);

            if (userType != (int)TipoUsuarioEnum.Administrador && userType != (int)TipoUsuarioEnum.RecursosHumanos)
            {
                query = query.Where(p => p.IdUsuarioSolicita == userId);
            }

            var stats = await query
              .GroupBy(p => 1)
              .Select(g => new
              {
                  Total = g.Count(),
                  Pendientes = g.Count(p => !p.Revisado),
                  Aprobados = g.Count(p => p.Revisado),
                  ConGoce = g.Count(p => p.Goce),
                  SinGoce = g.Count(p => !p.Goce),
                  EsteMes = g.Count(p => p.FechaCreacion.Month == DateTime.Now.Month && p.FechaCreacion.Year == DateTime.Now.Year)
              })
              .FirstOrDefaultAsync();

            return stats ?? new { Total = 0, Pendientes = 0, Aprobados = 0, ConGoce = 0, SinGoce = 0, EsteMes = 0 };
        }

        public async Task<int> CreatePermisoAsync(Permiso permiso, int idUsuarioCrea)
        {
            var corteActivo = await _context.Cortes
                .Where(c => c.Estatus)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (corteActivo == null) throw new InvalidOperationException("No se encontró un corte activo.");

            permiso.IdCorte = corteActivo.Id;
            permiso.IdUsuarioCrea = idUsuarioCrea;
            permiso.IdUsuarioModifica = idUsuarioCrea;
            permiso.FechaCreacion = DateTime.Now;
            permiso.FechaModificacion = DateTime.Now;
            permiso.Estatus = true;
            permiso.Revisado = false;

            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();
            return permiso.Id;
        }

        public async Task<bool> UpdatePermisoAsync(Permiso permiso, int idUsuarioModifica)
        {
            var existing = await _context.Permisos
                .Include(p => p.IdCorteNavigation)
                .FirstOrDefaultAsync(p => p.Id == permiso.Id);

            if (existing == null) return false;
            
            // Reglas de negocio
            if (existing.Revisado || (existing.IdCorteNavigation != null && !existing.IdCorteNavigation.Estatus))
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
            existing.Revisado = false; // Reset revision on update

            _context.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePermisoAsync(int id, int idUsuarioModifica)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null) return false;

            permiso.Estatus = false;
            permiso.IdUsuarioModifica = idUsuarioModifica;
            permiso.FechaModificacion = DateTime.Now;

            _context.Update(permiso);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AutorizarPermisoAsync(int id, int idUsuarioAutoriza, bool aprobado, string comentario)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null) return false;

            permiso.Revisado = true;
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
