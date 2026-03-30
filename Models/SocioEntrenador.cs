using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApi.Models;

public partial class SocioEntrenador
{
    [Key] // le dice a EF que es la PK
    public int SocioEntrenadorId { get; set; }

    public int SocioId { get; set; }

    public int EntrenadorId { get; set; }

    public DateOnly FechaAsignacion { get; set; }

    public bool Activo { get; set; }

    public virtual Entrenadore Entrenador { get; set; } = null!;

    public virtual Socio Socio { get; set; } = null!;
}
