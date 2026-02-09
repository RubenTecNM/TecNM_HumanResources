using System;

namespace RecursosHumanosWeb.Models.ViewModels.Usuario
{
    public class UsuarioDetailsViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public bool Estatus { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        public int IdTipoUsuario { get; set; }
        public string TipoUsuarioNombre { get; set; } = string.Empty;

        public int IdArea { get; set; }
        public string AreaDescripcion { get; set; } = string.Empty;

        public int IdDepartamento { get; set; }
        public string DepartamentoDescripcion { get; set; } = string.Empty;

        public int IdPuesto { get; set; }
        public string PuestoDescripcion { get; set; } = string.Empty;

        public int? IdUsuarioCrea { get; set; }
        public string? UsuarioCreaNombre { get; set; }

        public int? IdUsuarioModifica { get; set; }
        public string? UsuarioModificaNombre { get; set; }

        public int? IdApiKey { get; set; }

        // Estadísticas de permisos
        public int TotalPermisos { get; set; }
        public int PermisosRevisados { get; set; }
        public int ConGoce { get; set; }
        public int SinGoce { get; set; }
        
        // Campos compatibles con el servicio
        public string TipoUsuario { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public string UsuarioCrea { get; set; } = string.Empty;
        public string UsuarioModifica { get; set; } = string.Empty;
    }
}