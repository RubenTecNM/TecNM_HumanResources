using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Helpers;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Models.ViewModels.User;
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
        private readonly HumanResourcesContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(HumanResourcesContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IPagedList<User>> GetUsuariosPagedAsync(
            string nombreFilter,
            string correoFilter,
            int? departamentoFilter,
            bool? estadoFilter,
            int page,
            int pageSize,
            string sortOrder)
        {
            var query = _context.Users
                .Include(u => u.Area)
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Include(u => u.UserType)
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(nombreFilter))
                query = query.Where(u => u.Name.Contains(nombreFilter));

            if (!string.IsNullOrEmpty(correoFilter))
                query = query.Where(u => u.Email.Contains(correoFilter));

            if (departamentoFilter.HasValue)
                query = query.Where(u => u.DepartmentId == departamentoFilter.Value);

            if (estadoFilter.HasValue)
                query = query.Where(u => u.Status == estadoFilter.Value);

            // Aplicar ordenamiento
            query = sortOrder switch
            {
                "nombre_desc" => query.OrderByDescending(u => u.Name ?? ""),
                "correo" => query.OrderBy(u => u.Email ?? ""),
                "correo_desc" => query.OrderByDescending(u => u.Email ?? ""),
                "department" => query.OrderBy(u => u.Department == null ? "" : (u.Department.Description ?? "")),
                "departamento_desc" => query.OrderByDescending(u => u.Department == null ? "" : (u.Department.Description ?? "")),
                "fecha" => query.OrderBy(u => u.CreatedAt),
                "fecha_desc" => query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderBy(u => u.Name ?? "")
            };

            return query.ToPagedList(page, pageSize);
        }

        public async Task<User> GetUsuarioByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Area)
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Include(u => u.UserType)
                .Include(u => u.CreatedByUser)
                .Include(u => u.UpdatedByUser)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserDetailsViewModel> GetUsuarioDetailsWithStatsAsync(int id)
        {
            var usuarioBasico = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Status,
                    u.CreatedAt,
                    u.UpdatedAt,
                    u.UserTypeId,
                    u.AreaId,
                    u.DepartmentId,
                    u.PositionId,
                    u.CreatedByUserId,
                    u.UpdatedByUserId,
                    UserType = u.UserType.Description,
                    Area = u.Area.Description,
                    Department = u.Department.Description,
                    Position = u.Position.Description,
                    CreatedBy = u.CreatedByUser.Name,
                    UpdatedBy = u.UpdatedByUser != null ? u.UpdatedByUser.Name : null
                })
                .FirstOrDefaultAsync();

            if (usuarioBasico == null) return null;

            // Obtener contadores de permissions
            var permisosStats = await _context.Permissions
                .Where(p => p.RequestingUserId == id && p.PermissionStatusTypeId != 3)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalPermisos = g.Count(),
                    PermisosRevisados = g.Count(p => p.PermissionStatusTypeId != 1),
                    WithPay = g.Count(p => p.WithPay),
                    SinGoce = g.Count(p => !p.WithPay)
                })
                .FirstOrDefaultAsync();

            return new UserDetailsViewModel
            {
                Id = usuarioBasico.Id,
                Name = usuarioBasico.Name,
                Email = usuarioBasico.Email,
                Status = usuarioBasico.Status,
                CreatedAt = usuarioBasico.CreatedAt,
                UpdatedAt = usuarioBasico.UpdatedAt,
                UserType = usuarioBasico.UserType,
                TipoUsuarioNombre = usuarioBasico.UserType, // Fix: Asignar a la propiedad usada en la vista
                Area = usuarioBasico.Area,
                AreaDescripcion = usuarioBasico.Area, // Fix: Asignar a la propiedad usada en la vista (si se usara)
                Department = usuarioBasico.Department,
                DepartamentoDescripcion = usuarioBasico.Department, // Fix: Asignar a la propiedad usada en la vista
                Position = usuarioBasico.Position,
                PuestoDescripcion = usuarioBasico.Position, // Fix: Asignar a la propiedad usada en la vista
                CreatedBy = usuarioBasico.CreatedBy,
                UsuarioCreaNombre = usuarioBasico.CreatedBy, // Fix: Asignar a la propiedad usada en la vista
                UpdatedBy = usuarioBasico.UpdatedBy,
                UsuarioModificaNombre = usuarioBasico.UpdatedBy, // Fix: Asignar a la propiedad usada en la vista
                TotalPermisos = permisosStats?.TotalPermisos ?? 0,
                PermisosRevisados = permisosStats?.PermisosRevisados ?? 0,
                WithPay = permisosStats?.WithPay ?? 0,
                SinGoce = permisosStats?.SinGoce ?? 0
            };
        }

        public async Task<int> CreateUsuarioAsync(User user, int idUsuarioCrea, string claveTextoPlano)
        {
            user.Status = true;
            user.CreatedAt = DateTime.Now;
            user.CreatedByUserId = idUsuarioCrea;
            user.UpdatedByUserId = idUsuarioCrea;
            user.ApiKeyId = null;
            user.PasswordHash = string.IsNullOrWhiteSpace(claveTextoPlano) 
                ? string.Empty 
                : PasswordHasher.HashSha256(claveTextoPlano);

            _context.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task<bool> UpdateUsuarioAsync(User user, int idUsuarioModifica, string? claveTextoPlano = null)
        {
            var existing = await _context.Users.FindAsync(user.Id);
            if (existing == null) return false;

            // Actualizar campos
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.UserTypeId = user.UserTypeId;
            existing.AreaId = user.AreaId;
            existing.DepartmentId = user.DepartmentId;
            existing.PositionId = user.PositionId;
            existing.Status = user.Status;
            existing.UpdatedByUserId = idUsuarioModifica;
            existing.UpdatedAt = DateTime.Now;

            // Solo actualizar clave si se proporciona
            if (!string.IsNullOrWhiteSpace(claveTextoPlano))
            {
                existing.PasswordHash = PasswordHasher.HashSha256(claveTextoPlano);
            }

            _context.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUsuarioAsync(int id, int idUsuarioModifica)
        {
            // Validar que el user no se esté eliminando a sí mismo
            if (id == idUsuarioModifica)
            {
                _logger.LogWarning("User {Id} intentó eliminarse a sí mismo", id);
                return false;
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Status = false;
            user.UpdatedByUserId = idUsuarioModifica;
            user.UpdatedAt = DateTime.Now;

            _context.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<UsersDTO>> SearchUsuariosAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<UsersDTO>();

            return await _context.Users
                .Where(u => u.Status && (u.Name.Contains(term) || u.Email.Contains(term)))
                .OrderBy(u => u.Name)
                .Take(20)
                .Select(u => new UsersDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserTypeId = u.UserTypeId,
                    AreaId = u.AreaId,
                    DepartmentId = u.DepartmentId
                })
                .ToListAsync();
        }

        public async Task<List<Area>> GetAreasAsync()
        {
            return await _context.Areas
                .Where(a => a.Status)
                .OrderBy(a => a.Description)
                .ToListAsync();
        }

        public async Task<List<Department>> GetDepartamentosAsync()
        {
            return await _context.Departments
                .Where(d => d.Status)
                .OrderBy(d => d.Description)
                .ToListAsync();
        }

        public async Task<List<Position>> GetPuestosAsync()
        {
            return await _context.Positions
                .Where(p => p.Status)
                .OrderBy(p => p.Description)
                .ToListAsync();
        }

        public async Task<List<UserType>> GetTipoUsuariosAsync()
        {
            return await _context.UserTypes
                .OrderBy(t => t.Description)
                .ToListAsync();
        }

        public async Task<List<User>> GetActiveUsuariosAsync()
        {
            return await _context.Users
                .Where(u => u.Status)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public bool CanUserManageUsuarios(int userType)
        {
            return userType == (int)UserTypeEnum.Administrator || 
                   userType == (int)UserTypeEnum.HumanResources;
        }

        public bool UsuarioExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
