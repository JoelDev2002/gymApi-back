public class ActualizarRutinaRequest
{
  public int SocioId {get;set;}
  public int EntrenadorId {get;set;}
  public string Nombre {get;set;}="";
  public string Objetivo {get;set;}="";
  public DateOnly FechaInicio {get;set;}
  public DateOnly FechaFin {get;set;}
}