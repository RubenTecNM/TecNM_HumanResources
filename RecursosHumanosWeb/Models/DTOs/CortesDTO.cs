namespace RecursosHumanosWeb.Models.DTOs
{
    public class CortesDTO
    {
        public int Id { get; set; }

        public DateTime Inicia { get; set; }

        public DateTime Termina { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public int IdEstadoCorte { get; set; }
        public string EstadoDescripcion { get; set; }
        public string EstadoColor { get; set; }

        public int IdUsuarioCrea { get; set; }

        public int? IdUsuarioModifica { get; set; }
    }
}
