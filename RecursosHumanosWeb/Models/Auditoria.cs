using System;
using System.Collections.Generic;

namespace RecursosHumanosWeb.Models;

public partial class Auditoria
{
    public int Id { get; set; }

    public int IdTabla { get; set; }

    public int IdRegistro { get; set; }

    public int IdApiFunction { get; set; }

    public int? IdApiKey { get; set; }
    
    public int IdTipoOrigen { get; set; }

    public string? DatosAnteriores { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Estatus { get; set; }

    public int IdUsuarioCrea { get; set; }

    public int? IdUsuarioModifica { get; set; }

    public virtual ApiFunction IdApiFunctionNavigation { get; set; } = null!;

    public virtual ApiKey? IdApiKeyNavigation { get; set; }
    
    public virtual TipoOrigen IdTipoOrigenNavigation { get; set; } = null!;

    public virtual Tabla IdTablaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioCreaNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioModificaNavigation { get; set; }
}
