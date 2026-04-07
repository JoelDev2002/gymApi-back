using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
  [Required(ErrorMessage ="campo username es obligatorio")]
  [EmailAddress(ErrorMessage ="Ingresae un correo valido")]
  public string Email {get;set;}="";

  [Required(ErrorMessage ="campo username es obligatorio")]
  [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres")]
  public string Password {get;set;}="";
}