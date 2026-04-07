using System.ComponentModel.DataAnnotations;

public class CrearRutinaEjercicioRequest
{
  [Required(ErrorMessage ="campo ejercicio es obligatorio")]
  public int EjercicioId { get; set; }
  [Required(ErrorMessage ="campo orden es obligatorio")]
  public int Orden { get; set; }
  [Required(ErrorMessage ="campo series es obligatorio")]
  public int? Series { get; set; }
  [Required(ErrorMessage ="campo repeticion es obligatorio")]
  public int? Repeticiones { get; set; }

  [Required(ErrorMessage ="campo peso es obligatorio")]
  public decimal? PesoObjetivoKg { get; set; }
  [Required(ErrorMessage ="campo duracion es obligatorio")]
  public int? DuracionSegundos { get; set; }
  [Required(ErrorMessage ="campo descanso es obligatorio")]
  public int? DescansoSegundos { get; set; }
  public string? Notas { get; set; }
}