using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels
{
    public class PermissionCreateViewModel
    {
        [Required]
        public int PermissionTypeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? Days { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }

        [Required]
        public bool WithPay { get; set; }

        public IEnumerable<TipoPermisoViewModel> TiposPermiso { get; set; }
    }

    public class TipoPermisoViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}