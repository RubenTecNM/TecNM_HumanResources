using RecursosHumanosWeb.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using X.PagedList;
using System.Collections.Generic;

namespace RecursosHumanosWeb.Models.ViewModels
{
    public class SearchPermissionsViewModel
    {
        public IPagedList<PermissionsDTO> Permissions { get; set; }
        public List<UsersDTO> Users { get; set; } = new List<UsersDTO>();
        public List<AreasDTO> Areas { get; set; } = new List<AreasDTO>();
        public string SelectedUser { get; set; }
        public int? SelectedArea { get; set; }
        public bool? Revisado { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? UserName { get; set; }
        public int? UserId { get; set; }
        public int? AreaId { get; set; }
        public int? PermissionTypeId { get; set; }

        // Resultados
        public string? Reason { get; internal set; }
        public int Id { get; internal set; }
    }
}
