using X.PagedList;

namespace RecursosHumanosWeb.Models.ViewModels
{
    public class SearchUserViewModel
    {
        public IPagedList<RecursosHumanosWeb.Models.User>? UsuariosPaginados { get; set; }
        public IEnumerable<RecursosHumanosWeb.Models.User> Users { get; set; } = new List<RecursosHumanosWeb.Models.User>();
        public IEnumerable<Area> Areas { get; set; } = new List<Area>();
        public IEnumerable<Department> Departments { get; set; } = new List<Department>();
    }
}
