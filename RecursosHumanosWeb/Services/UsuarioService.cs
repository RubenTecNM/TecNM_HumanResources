using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Helpers;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Models.ViewModels.Usuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace RecursosHumanosWeb.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly RecursosHumanosContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(RecursosHumanosContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IPagedList<Usuario>> GetUsuariosPagedAsync(
            string nombreFilter,
            string correoFilter,
            int? departamentoFilter,
            bool? estadoFilter,
            int page,
            int pageSize,
            string sortOrder)
        {
            var query = _context.Usuarios
                .Include(u => u.IdAreaNavigation)
                .Include(u => u.IdDepartamentoNavigation)
                .Include(u => u.IdPuestoNavigation)
                .Include(u => u.IdTipoUsuarioNavigation)
                .Include(u => u.IdUsuarioCreaNavigation)
                .Include(u => u.IdUsuarioModificaNavigation)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(nombreFilter))
                query = query.Where(u => u.Nombre.Contains(nombreFilter));

            if (!string.IsNullOrEmpty(correoFilter))
                query = query.Where(u => u.Correo.Contains(correoFilter));

            if (departamentoFilter.HasValue)
                query = query.Where(u => u.IdDepartamento == departamentoFilter.Value);

            if (estadoFilter.HasValue)
                query = query.Where(u => u.Estatus == estadoFilter.Value);

            // Aplicar ordenamiento
            query = sortOrder switch
            {
                "nombre_desc" => query.OrderByDescending(u => u.Nombre ?? ""),
                "correo" => query.OrderBy(u => u.Correo ?? ""),
                "correo_desc" => query.OrderByDescending(u => u.Correo ?? ""),
                "departamento" => query.OrderBy(u => u.IdDepartamentoNavigation == null ? "" : (u.IdDepartamentoNavigation.Descripcion ?? "")),
                "departamento_desc" => query.OrderByDescending(u => u.IdDepartamentoNavigation == null ? "" : (u.IdDepartamentoNavigation.Descripcion ?? "")),
                "fecha" => query.OrderBy(u => u.FechaCreacion),
                "fecha_desc" => query.OrderByDescending(u => u.FechaCreacion),
                _ => query.OrderBy(u => u.Nombre ?? "")
            };

            return query.ToPagedList(page, pageSize);
        }

        public async Task<Usuario> GetUsuarioByIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.IdAreaNavigation)
                .Include(u => u.IdDepartamentoNavigation)
                .Include(u => u.IdPuestoNavigation)
                .Include(u => u.IdTipoUsuarioNavigation)
                .Include(u => u.IdUsuarioCreaNavigation)
                .Include(u => u.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UsuarioDetailsViewModel> GetUsuarioDetailsWithStatsAsync(int id)
        {
            var usuarioBasico = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Correo,
                    u.Estatus,
                    u.FechaCreacion,
                    u.FechaModificacion,
                    u.IdTipoUsuario,
                    u.IdArea,
                    u.IdDepartamento,
                    u.IdPuesto,
                    u.IdUsuarioCrea,
                    u.IdUsuarioModifica,
                    TipoUsuario = u.IdTipoUsuarioNavigation.Descripcion,
                    Area = u.IdAreaNavigation.Descripcion,
                    Departamento = u.IdDepartamentoNavigation.Descripcion,
                    Puesto = u.IdPuestoNavigation.Descripcion,
                    UsuarioCrea = u.IdUsuarioCreaNavigation.Nombre,
                    UsuarioModifica = u.IdUsuarioModificaNavigation != null ? u.IdUsuarioModificaNavigation.Nombre : null
                })
                .FirstOrDefaultAsync();

            if (usuarioBasico == null) return null;

            // Obtener contadores de permisos
            var permisosStats = await _context.Permisos
                .Where(p => p.IdUsuarioSolicita == id && p.IdEstadoPermiso != 3)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalPermisos = g.Count(),
                    PermisosRevisados = g.Count(p => p.IdEstadoPermiso != 1),
                    ConGoce = g.Count(p => p.Goce),
                    SinGoce = g.Count(p => !p.Goce)
                })
                .FirstOrDefaultAsync();

            return new UsuarioDetailsViewModel
            {
                Id = usuarioBasico.Id,
                Nombre = usuarioBasico.Nombre,
                Correo = usuarioBasico.Correo,
                Estatus = usuarioBasico.Estatus,
                FechaCreacion = usuarioBasico.FechaCreacion,
                FechaModificacion = usuarioBasico.FechaModificacion,
                TipoUsuario = usuarioBasico.TipoUsuario,
                TipoUsuarioNombre = usuarioBasico.TipoUsuario, // Fix: Asignar a la propiedad usada en la vista
                Area = usuarioBasico.Area,
                AreaDescripcion = usuarioBasico.Area, // Fix: Asignar a la propiedad usada en la vista (si se usara)
                Departamento = usuarioBasico.Departamento,
                DepartamentoDescripcion = usuarioBasico.Departamento, // Fix: Asignar a la propiedad usada en la vista
                Puesto = usuarioBasico.Puesto,
                PuestoDescripcion = usuarioBasico.Puesto, // Fix: Asignar a la propiedad usada en la vista
                UsuarioCrea = usuarioBasico.UsuarioCrea,
                UsuarioCreaNombre = usuarioBasico.UsuarioCrea, // Fix: Asignar a la propiedad usada en la vista
                UsuarioModifica = usuarioBasico.UsuarioModifica,
                UsuarioModificaNombre = usuarioBasico.UsuarioModifica, // Fix: Asignar a la propiedad usada en la vista
                TotalPermisos = permisosStats?.TotalPermisos ?? 0,
                PermisosRevisados = permisosStats?.PermisosRevisados ?? 0,
                ConGoce = permisosStats?.ConGoce ?? 0,
                SinGoce = permisosStats?.SinGoce ?? 0
            };
        }

        public async Task<int> CreateUsuarioAsync(Usuario usuario, int idUsuarioCrea, string claveTextoPlano)
        {
            usuario.Estatus = true;
            usuario.FechaCreacion = DateTime.Now;
            usuario.IdUsuarioCrea = idUsuarioCrea;
            usuario.IdUsuarioModifica = idUsuarioCrea;
            usuario.IdApiKey = null;
            usuario.Clave = string.IsNullOrWhiteSpace(claveTextoPlano) 
                ? string.Empty 
                : PasswordHasher.HashSha256(claveTextoPlano);

            _context.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario.Id;
        }

        public async Task<bool> UpdateUsuarioAsync(Usuario usuario, int idUsuarioModifica, string? claveTextoPlano = null)
        {
            var existing = await _context.Usuarios.FindAsync(usuario.Id);
            if (existing == null) return false;

            // Actualizar campos
            existing.Nombre = usuario.Nombre;
            existing.Correo = usuario.Correo;
            existing.IdTipoUsuario = usuario.IdTipoUsuario;
            existing.IdArea = usuario.IdArea;
            existing.IdDepartamento = usuario.IdDepartamento;
            existing.IdPuesto = usuario.IdPuesto;
            existing.Estatus = usuario.Estatus;
            existing.IdUsuarioModifica = idUsuarioModifica;
            existing.FechaModificacion = DateTime.Now;

            // Solo actualizar clave si se proporciona
            if (!string.IsNullOrWhiteSpace(claveTextoPlano))
            {
                existing.Clave = PasswordHasher.HashSha256(claveTextoPlano);
            }

            _context.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUsuarioAsync(int id, int idUsuarioModifica)
        {
            // Validar que el usuario no se esté eliminando a sí mismo
            if (id == idUsuarioModifica)
            {
                _logger.LogWarning("Usuario {Id} intentó eliminarse a sí mismo", id);
                return false;
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            usuario.Estatus = false;
            usuario.IdUsuarioModifica = idUsuarioModifica;
            usuario.FechaModificacion = DateTime.Now;

            _context.Update(usuario);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<UsuariosDTO>> SearchUsuariosAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<UsuariosDTO>();

            return await _context.Usuarios
                .Where(u => u.Estatus && (u.Nombre.Contains(term) || u.Correo.Contains(term)))
                .OrderBy(u => u.Nombre)
                .Take(20)
                .Select(u => new UsuariosDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    IdTipoUsuario = u.IdTipoUsuario,
                    IdArea = u.IdArea,
                    IdDepartamento = u.IdDepartamento
                })
                .ToListAsync();
        }

        public async Task<List<Area>> GetAreasAsync()
        {
            return await _context.Areas
                .Where(a => a.Estatus)
                .OrderBy(a => a.Descripcion)
                .ToListAsync();
        }

        public async Task<List<Departamento>> GetDepartamentosAsync()
        {
            return await _context.Departamentos
                .Where(d => d.Estatus)
                .OrderBy(d => d.Descripcion)
                .ToListAsync();
        }

        public async Task<List<Puesto>> GetPuestosAsync()
        {
            return await _context.Puestos
                .Where(p => p.Estatus)
                .OrderBy(p => p.Descripcion)
                .ToListAsync();
        }

        public async Task<List<TipoUsuario>> GetTipoUsuariosAsync()
        {
            return await _context.TipoUsuarios
                .OrderBy(t => t.Descripcion)
                .ToListAsync();
        }

        public async Task<List<Usuario>> GetActiveUsuariosAsync()
        {
            return await _context.Usuarios
                .Where(u => u.Estatus)
                .OrderBy(u => u.Nombre)
                .ToListAsync();
        }

        public bool CanUserManageUsuarios(int userType)
        {
            return userType == (int)TipoUsuarioEnum.Administrador || 
                   userType == (int)TipoUsuarioEnum.RecursosHumanos;
        }

        public bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
