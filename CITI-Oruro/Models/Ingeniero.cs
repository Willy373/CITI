using System;
using System.Collections.Generic;

namespace CITI_Oruro.Models;

public partial class Ingeniero
{
    public int IdIngeniero { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Especialidad { get; set; } = null!;

    public int Rni { get; set; }

    public int Ci { get; set; }

    public DateTime Fecha { get; set; }

    public virtual ICollection<Cuotum> Cuota { get; set; } = new List<Cuotum>();

    public virtual ICollection<DetalleInscripcion> DetalleInscripcions { get; set; } = new List<DetalleInscripcion>();
}
