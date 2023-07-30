using System;
using System.Collections.Generic;

namespace CITI_Oruro.Models;

public partial class Inscripcion
{
    public int IdInscripcion { get; set; }

    public int Monto { get; set; }

    public string Tipo { get; set; } = null!;

    public virtual ICollection<DetalleInscripcion> DetalleInscripcions { get; set; } = new List<DetalleInscripcion>();
}
