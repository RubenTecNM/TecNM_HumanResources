using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace RecursosHumanosWeb.Services
{
    public interface IUserService
    {
        Task<IPagedList<User>> GetUsersPagedAsync(
            string nameFilter,
            string emailFilter,
            int? departmentFilter,
            bool? statusFilter,
            int page,
            int pageSize,
            string sortOrder);

        Task<User> GetUserByIdAsync(int id);

        Task<UserDetailsViewModel> GetUserDetailsWithStatsAsync(int id);

        Task<int> CreateUserAsync(User user, int createdByUserId, string plainPassword);

        Task<bool> UpdateUserAsync(User user, int updatedByUserId, string? plainPassword = null);

        Task<bool> DeleteUserAsync(int id, int updatedByUserId);

        Task<List<UsersDTO>> SearchUsersAsync(string term);

        Task<List<Area>> GetAreasAsync();
        Task<List<Department>> GetDepartmentsAsync();
        Task<List<Position>> GetPositionsAsync();
        Task<List<UserType>> GetUserTypesAsync();
        Task<List<User>> GetActiveUsersAsync();

        bool CanUserManageUsers(int userType);

        bool UserExists(int id);
    }
}
