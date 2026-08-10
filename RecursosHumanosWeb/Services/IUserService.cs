using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace RecursosHumanosWeb.Services
{
    public interface IUsuarioService
    {
        Task<IPagedList<User>> GetUsuariosPagedAsync(
            string nombreFilter,
            string correoFilter,
            int? departamentoFilter,
            bool? estadoFilter,
            int page,
            int pageSize,
            string sortOrder);

        Task<User> GetUsuarioByIdAsync(int id);

        Task<UserDetailsViewModel> GetUsuarioDetailsWithStatsAsync(int id);

        Task<int> CreateUsuarioAsync(User user, int idUsuarioCrea, string claveTextoPlano);

        Task<bool> UpdateUsuarioAsync(User user, int idUsuarioModifica, string? claveTextoPlano = null);

        Task<bool> DeleteUsuarioAsync(int id, int idUsuarioModifica);

        Task<List<UsersDTO>> SearchUsuariosAsync(string term);

        Task<List<Area>> GetAreasAsync();
        Task<List<Department>> GetDepartamentosAsync();
        Task<List<Position>> GetPuestosAsync();
        Task<List<UserType>> GetTipoUsuariosAsync();
        Task<List<User>> GetActiveUsuariosAsync();

        bool CanUserManageUsuarios(int userType);

        bool UsuarioExists(int id);
    }
}
