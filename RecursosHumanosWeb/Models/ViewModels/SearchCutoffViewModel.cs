using RecursosHumanosWeb.Models.DTOs;
using X.PagedList;

namespace RecursosHumanosWeb.Models.ViewModels
{
    public class SearchCutoffViewModel
    {
        public IPagedList<CutoffsDTO> Cutoffs { get; set; }
        public List<UsersDTO> Users { get; set; }
        public string FechaDesde { get; set; }
        public string FechaHasta { get; set; }
        public int? StatusId { get; set; }
        public bool IsAdministrador { get; set; }
    }
}
