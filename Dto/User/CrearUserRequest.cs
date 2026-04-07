using System.ComponentModel.DataAnnotations;

public class CrearUserRequest
{
    [Required(ErrorMessage ="campo username obligatorio")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage ="campo email obligatorio")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage ="campo contraseña obligatorio")]
    public string Password { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public List<int> Roles {get;set;}=null!;
}