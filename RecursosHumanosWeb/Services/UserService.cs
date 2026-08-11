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
    public class UserService : IUserService
    {
        private readonly HumanResourcesContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(HumanResourcesContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IPagedList<User>> GetUsersPagedAsync(
            string nameFilter,
            string emailFilter,
            int? departmentFilter,
            bool? statusFilter,
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
            if (!string.IsNullOrEmpty(nameFilter))
                query = query.Where(u => u.Name.Contains(nameFilter));

            if (!string.IsNullOrEmpty(emailFilter))
                query = query.Where(u => u.Email.Contains(emailFilter));

            if (departmentFilter.HasValue)
                query = query.Where(u => u.DepartmentId == departmentFilter.Value);

            if (statusFilter.HasValue)
                query = query.Where(u => u.Status == statusFilter.Value);

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

        public async Task<User> GetUserByIdAsync(int id)
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

        public async Task<UserDetailsViewModel> GetUserDetailsWithStatsAsync(int id)
        {
            var basicUser = await _context.Users
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

            if (basicUser == null) return null;

            // Obtener contadores de permissions
            var permissionStats = await _context.Permissions
                .Where(p => p.RequestingUserId == id && p.PermissionStatusTypeId != (int)PermissionStatusEnum.InvalidatedByPeriod)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalPermissions = g.Count(),
                    ReviewedPermissions = g.Count(p => p.PermissionStatusTypeId != (int)PermissionStatusEnum.Pending),
                    WithPay = g.Count(p => p.WithPay),
                    WithoutPay = g.Count(p => !p.WithPay)
                })
                .FirstOrDefaultAsync();

            return new UserDetailsViewModel
            {
                Id = basicUser.Id,
                Name = basicUser.Name,
                Email = basicUser.Email,
                Status = basicUser.Status,
                CreatedAt = basicUser.CreatedAt,
                UpdatedAt = basicUser.UpdatedAt,
                UserType = basicUser.UserType,
                UserTypeName = basicUser.UserType, // Fix: Asignar a la propiedad usada en la vista
                Area = basicUser.Area,
                AreaDescription = basicUser.Area, // Fix: Asignar a la propiedad usada en la vista (si se usara)
                Department = basicUser.Department,
                DepartmentDescription = basicUser.Department, // Fix: Asignar a la propiedad usada en la vista
                Position = basicUser.Position,
                PositionDescription = basicUser.Position, // Fix: Asignar a la propiedad usada en la vista
                CreatedBy = basicUser.CreatedBy,
                CreatedByName = basicUser.CreatedBy, // Fix: Asignar a la propiedad usada en la vista
                UpdatedBy = basicUser.UpdatedBy,
                UpdatedByName = basicUser.UpdatedBy, // Fix: Asignar a la propiedad usada en la vista
                TotalPermissions = permissionStats?.TotalPermissions ?? 0,
                ReviewedPermissions = permissionStats?.ReviewedPermissions ?? 0,
                WithPay = permissionStats?.WithPay ?? 0,
                WithoutPay = permissionStats?.WithoutPay ?? 0
            };
        }

        public async Task<int> CreateUserAsync(User user, int createdByUserId, string plainPassword)
        {
            user.Status = true;
            user.CreatedAt = DateTime.Now;
            user.CreatedByUserId = createdByUserId;
            user.UpdatedByUserId = createdByUserId;
            user.ApiKeyId = null;
            user.PasswordHash = string.IsNullOrWhiteSpace(plainPassword) 
                ? string.Empty 
                : PasswordHasher.HashSha256(plainPassword);

            _context.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task<bool> UpdateUserAsync(User user, int updatedByUserId, string? plainPassword = null)
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
            existing.UpdatedByUserId = updatedByUserId;
            existing.UpdatedAt = DateTime.Now;

            // Solo actualizar clave si se proporciona
            if (!string.IsNullOrWhiteSpace(plainPassword))
            {
                existing.PasswordHash = PasswordHasher.HashSha256(plainPassword);
            }

            _context.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUserAsync(int id, int updatedByUserId)
        {
            // Validar que el user no se esté eliminando a sí mismo
            if (id == updatedByUserId)
            {
                _logger.LogWarning("User {Id} intentó eliminarse a sí mismo", id);
                return false;
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Status = false;
            user.UpdatedByUserId = updatedByUserId;
            user.UpdatedAt = DateTime.Now;

            _context.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<UsersDTO>> SearchUsersAsync(string term)
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

        public async Task<List<Department>> GetDepartmentsAsync()
        {
            return await _context.Departments
                .Where(d => d.Status)
                .OrderBy(d => d.Description)
                .ToListAsync();
        }

        public async Task<List<Position>> GetPositionsAsync()
        {
            return await _context.Positions
                .Where(p => p.Status)
                .OrderBy(p => p.Description)
                .ToListAsync();
        }

        public async Task<List<UserType>> GetUserTypesAsync()
        {
            return await _context.UserTypes
                .OrderBy(t => t.Description)
                .ToListAsync();
        }

        public async Task<List<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Status)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public bool CanUserManageUsers(int userType)
        {
            return userType == (int)UserTypeEnum.Administrator || 
                   userType == (int)UserTypeEnum.HumanResources;
        }

        public bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
