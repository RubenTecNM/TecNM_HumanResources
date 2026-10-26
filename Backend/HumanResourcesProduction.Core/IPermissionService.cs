using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.DTOs;
using HumanResourcesProduction.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace HumanResourcesProduction.Services
{
    public interface IPermissionService
    {
        Task<IPagedList<PermissionsDTO>> GetPermissionsPagedAsync(int userId, int? areaId, string userName, DateTime? fromDay, DateTime? toDay, int? statusId, int page, int pageSize);
        Task<object> GetStatsAsync(int userId, int userType);
        Task<int> CreatePermissionAsync(Permission permission, int createdByUserId);
        Task<bool> UpdatePermissionAsync(Permission permission, int updatedByUserId);
        Task<bool> CancelPermissionAsync(int id, int updatedByUserId);
        Task<bool> AuthorizePermissionAsync(int id, int authorizingUserId, bool aprobado, bool withPay, string? comment);
        bool CanUserAccessPermiso(int userId, int userType, Permission permission);
        Task<PermissionCreateConfigDto> GetCreateConfigurationAsync(int userId, int userType);
        Task<PermissionEditConfigDto> GetEditConfigurationAsync(int id, int userId, int userType);
        Task<PermissionAuthorizeConfigDto> GetAuthorizeConfigurationAsync(int id, int userId, int userType);
    }
}
