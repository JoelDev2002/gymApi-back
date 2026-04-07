using System.ComponentModel.DataAnnotations;

public class ActualizarEjercicioRequest
{
  [Required]
  [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres")]
  public string Nombre {get;set;} ="";
  public string Descripcion {get;set;} ="";
  
  public string GrupoMuscular {get;set;} ="";
}