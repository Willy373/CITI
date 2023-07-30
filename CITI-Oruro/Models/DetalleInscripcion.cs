using System;
using System.Collections.Generic;

namespace CITI_Oruro.Models;

public partial class DetalleInscripcion
{
    public int IdDetalleInscripcion { get; set; }

    public DateTime Fecha { get; set; }

    public int Total { get; set; }

    public int IdUsuario { get; set; }

    public int IdIngeniero { get; set; }

    public int IdInscripcion { get; set; }

    public virtual Ingeniero IdIngenieroNavigation { get; set; } = null!;

    public virtual Inscripcion IdInscripcionNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
