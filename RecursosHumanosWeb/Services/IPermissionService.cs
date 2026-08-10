using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace RecursosHumanosWeb.Services
{
    public interface IPermisoService
    {
        Task<IPagedList<PermissionsDTO>> GetPermisosPagedAsync(int userId, int? areaId, string userName, DateTime? fromDay, DateTime? toDay, bool? revisado, int page, int pageSize);
        Task<object> GetStatsAsync(int userId, int userType);
        Task<int> CreatePermisoAsync(Permission permission, int idUsuarioCrea);
        Task<bool> UpdatePermisoAsync(Permission permission, int idUsuarioModifica);
        Task<bool> DeletePermisoAsync(int id, int idUsuarioModifica);
        Task<bool> AutorizarPermisoAsync(int id, int idUsuarioAutoriza, bool aprobado, string comentario);
        bool CanUserAccessPermiso(int userId, int userType, Permission permission);
    }
}
