using System;
using System.Collections.Generic;

namespace CITI_Oruro.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Cuenta { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public bool EstadoUsuario { get; set; }

    public virtual ICollection<Cuotum> Cuota { get; set; } = new List<Cuotum>();

    public virtual ICollection<DetalleInscripcion> DetalleInscripcions { get; set; } = new List<DetalleInscripcion>();
}
