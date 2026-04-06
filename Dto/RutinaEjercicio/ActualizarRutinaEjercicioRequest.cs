public class ActualizarRutinaEjercicioRequest
{
  public int Orden { get; set; }
  public int? Series { get; set; }
  public int? Repeticiones { get; set; }
  public decimal? PesoObjetivoKg { get; set; }
  public int? DuracionSegundos { get; set; }
  public int? DescansoSegundos { get; set; }
  public string? Notas { get; set; }
}