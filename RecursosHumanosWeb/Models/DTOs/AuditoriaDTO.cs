using System;

namespace RecursosHumanosWeb.Models.DTOs
{
    public class AuditoriaDTO
    {
        public int Id { get; set; }
        public int IdRegistro { get; set; }
        public string Tabla { get; set; }
        public string Accion { get; set; }
        public string? ApiKeyDesc { get; set; }
        public string? DatosAnteriores { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Usuario { get; set; }
    }
}
