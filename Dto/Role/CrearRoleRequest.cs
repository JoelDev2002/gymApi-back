using System.ComponentModel.DataAnnotations;

public class CrearRoleRequest
{
  [Required(ErrorMessage ="campo Nombre es obligatorio")]
  [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres")]
  public string Name { get; set; } = null!;

}