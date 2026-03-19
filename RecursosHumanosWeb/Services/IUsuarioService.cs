using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels.Usuario;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace RecursosHumanosWeb.Services
{
    public interface IUsuarioService
    {
        Task<IPagedList<Usuario>> GetUsuariosPagedAsync(
            string nombreFilter,
            string correoFilter,
            int? departamentoFilter,
            bool? estadoFilter,
            int page,
            int pageSize,
            string sortOrder);

        Task<Usuario> GetUsuarioByIdAsync(int id);

        Task<UsuarioDetailsViewModel> GetUsuarioDetailsWithStatsAsync(int id);

        Task<int> CreateUsuarioAsync(Usuario usuario, int idUsuarioCrea, string claveTextoPlano);

        Task<bool> UpdateUsuarioAsync(Usuario usuario, int idUsuarioModifica, string? claveTextoPlano = null);

        Task<bool> DeleteUsuarioAsync(int id, int idUsuarioModifica);

        Task<List<UsuariosDTO>> SearchUsuariosAsync(string term);

        Task<List<Area>> GetAreasAsync();
        Task<List<Departamento>> GetDepartamentosAsync();
        Task<List<Puesto>> GetPuestosAsync();
        Task<List<TipoUsuario>> GetTipoUsuariosAsync();
        Task<List<Usuario>> GetActiveUsuariosAsync();

        bool CanUserManageUsuarios(int userType);

        bool UsuarioExists(int id);
    }
}
