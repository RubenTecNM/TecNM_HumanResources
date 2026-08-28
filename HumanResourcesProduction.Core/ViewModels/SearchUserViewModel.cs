using X.PagedList;

namespace HumanResourcesProduction.Models.ViewModels
{
    public class SearchUserViewModel
    {
        public IPagedList<HumanResourcesProduction.Models.User>? UsuariosPaginados { get; set; }
        public IEnumerable<HumanResourcesProduction.Models.User> Users { get; set; } = new List<HumanResourcesProduction.Models.User>();
        public IEnumerable<Area> Areas { get; set; } = new List<Area>();
        public IEnumerable<Department> Departments { get; set; } = new List<Department>();
    }
}
