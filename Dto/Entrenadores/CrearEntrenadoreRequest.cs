using System.ComponentModel.DataAnnotations;

public class CrearEntrenadoreRequest
{
  [Required(ErrorMessage ="campo username es obligatorio")]
  [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres")]
  public string UserName { get; set; }="";

  [Required(ErrorMessage ="campo email es obligatorio")]
  [EmailAddress(ErrorMessage ="Ingresae un correo valido")]
  public string Email { get; set; }="";

  [Required(ErrorMessage ="campo contraseña es obligatorio")]
  [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres")]
  public string Password { get; set; }="";

  [Phone]
  public string PhoneNumber { get; set; }="";


  [Required(ErrorMessage ="campo contraseña es obligatorio")]
  [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres")]
  public string Especialidad {get;set;}="";
  public string Certificaciones {get;set;}="";
}