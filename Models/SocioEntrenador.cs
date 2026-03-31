using System;
using System.Collections.Generic;

namespace GymApi.Models;

public partial class SocioEntrenador
{
    public int SocioId { get; set; }

    public int EntrenadorId { get; set; }

    public DateOnly FechaAsignacion { get; set; }

    public bool Activo { get; set; }

    public int SocioEntrenadorId { get; set; }

    public virtual Entrenadore Entrenador { get; set; } = null!;

    public virtual Socio Socio { get; set; } = null!;
}
