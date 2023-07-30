using System;
using System.Collections.Generic;

namespace CITI_Oruro.Models;

public partial class DetalleCuotum
{
    public int IdDetalleCuota { get; set; }

    public string DetalleMes { get; set; } = null!;

    public int Monto { get; set; }

    public int IdCuota { get; set; }

    public virtual Cuotum IdCuotaNavigation { get; set; } = null!;
}
