using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace RecursosHumanosWeb.Services
{
    public interface IPermissionService
    {
        Task<IPagedList<PermissionsDTO>> GetPermissionsPagedAsync(int userId, int? areaId, string userName, DateTime? fromDay, DateTime? toDay, bool? reviewed, int page, int pageSize);
        Task<object> GetStatsAsync(int userId, int userType);
        Task<int> CreatePermissionAsync(Permission permission, int createdByUserId);
        Task<bool> UpdatePermissionAsync(Permission permission, int updatedByUserId);
        Task<bool> DeletePermissionAsync(int id, int updatedByUserId);
        Task<bool> AuthorizePermissionAsync(int id, int authorizingUserId, bool aprobado, string comment);
        bool CanUserAccessPermiso(int userId, int userType, Permission permission);
    }
}
