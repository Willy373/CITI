using System;
using System.Collections.Generic;

namespace CITI_Oruro.Models;

public partial class Cuotum
{
    public int IdCuota { get; set; }

    public string? Detalle { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public int MesesTotal { get; set; }

    public int? Total { get; set; }

    public int IdUsuario { get; set; }

    public int IdIngeniero { get; set; }

    public virtual ICollection<DetalleCuotum> DetalleCuota { get; set; } = new List<DetalleCuotum>();

    public virtual Ingeniero? IdIngenieroNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioNavigation { get; set; } = null!;
}
