using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Permissions
{
    public class PermissionApiUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int PermissionTypeId { get; set; }
        
        public bool WithPay { get; set; }
    }
}
