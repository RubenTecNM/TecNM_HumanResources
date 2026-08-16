using X.PagedList;

namespace RecursosHumanosWeb.Models.ViewModels.User
{
    public class UserSearchViewModel
    {
        public IPagedList<Models.User>? PagedUsers { get; set; }
        public IEnumerable<Models.User> Users { get; set; } = new List<Models.User>();
        public IEnumerable<Area> Areas { get; set; } = new List<Area>();
        public IEnumerable<Department> Departments { get; set; } = new List<Department>();
    }
}
