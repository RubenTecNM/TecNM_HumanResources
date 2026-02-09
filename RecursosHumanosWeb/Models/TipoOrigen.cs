using System;
using System.Collections.Generic;

namespace RecursosHumanosWeb.Models;

public partial class TipoOrigen
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Estatus { get; set; }

    public int IdUsuarioCrea { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public virtual ICollection<Auditoria> Auditoria { get; set; } = new List<Auditoria>();

    public virtual Usuario IdUsuarioCreaNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioModificaNavigation { get; set; }
}

public static class TipoOrigenEnum
{
    public const int InterfazWeb = 1;
    public const int APIExterna = 2;
    public const int SistemaAutomatico = 3;
    public const int JobProgramado = 4;
    public const int SincronizacionLDAP = 5;
    public const int MigracionDatos = 6;
}
