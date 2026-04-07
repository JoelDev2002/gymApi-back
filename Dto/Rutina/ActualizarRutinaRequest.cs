using System.ComponentModel.DataAnnotations;

public class ActualizarRutinaRequest
{
  [Required(ErrorMessage ="campo socio es obligatorio")]
  public int SocioId {get;set;}
  [Required(ErrorMessage ="campo entrenador es obligatorio")]
  public int EntrenadorId {get;set;}

  [Required(ErrorMessage ="campo nombre es obligatorio")]
  public string Nombre {get;set;}="";
  public string Objetivo {get;set;}="";

  [Required(ErrorMessage ="campo fecha inicio es obligatorio")]
  public DateOnly FechaInicio {get;set;}

  [Required(ErrorMessage ="campo fecha fin es obligatorio")]
  public DateOnly FechaFin {get;set;}
}