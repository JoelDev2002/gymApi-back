using System.ComponentModel.DataAnnotations;

public class CrearMembresiaRequest
{
  [Required(ErrorMessage ="campo Nombre es obligatorio")]
  [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres")]
  public string Nombre {get;set;}="";

  [Required(ErrorMessage ="campo Descripcion es obligatorio")]
  [StringLength(100, MinimumLength = 3, ErrorMessage = "Entre 3 y 100 caracteres")]
  public string Descripcion {get;set;}="";

  [Required(ErrorMessage ="campo Duracion es obligatorio")]
  public int DuracionDias {get;set;}

  [Required(ErrorMessage ="campo Precio es obligatorio")]
  [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
  public decimal Precio {get;set;}

  [Required(ErrorMessage ="Seleccionar si es renovable o no")]
  public bool EsRenovable {get;set;}
}