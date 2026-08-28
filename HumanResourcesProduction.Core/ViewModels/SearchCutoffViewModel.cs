using HumanResourcesProduction.Models.DTOs;
using X.PagedList;

namespace HumanResourcesProduction.Models.ViewModels
{
    public class SearchCutoffViewModel
    {
        public IPagedList<CutoffsDTO> Cutoffs { get; set; }
        public List<UsersDTO> Users { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public int? StatusId { get; set; }
        public bool IsAdministrator { get; set; }
    }
}
