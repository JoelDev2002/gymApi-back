using System.ComponentModel.DataAnnotations;

public class CrearSocioRequest
{
    [Required(ErrorMessage ="campo username es obligatorio")]
    public string UserName { get; set; }="";
    [Required(ErrorMessage ="campo email es obligatorio")]
    public string Email { get; set; }="";
    [Required(ErrorMessage ="campo contraseña es obligatorio")]
    public string Password { get; set; }="";
    public string PhoneNumber { get; set; }="";

    [Required(ErrorMessage ="campo fecha nacimiento es obligatorio")]
    public DateOnly? FechaNacimiento { get; set; }
    [Required(ErrorMessage ="campo genero es obligatorio")]
    public string? Genero { get; set; }
    [Required(ErrorMessage ="campo altura es obligatorio")]
    public decimal? AlturaCm { get; set; }
    [Required(ErrorMessage ="campo peso es obligatorio")]
    public decimal? PesoKg { get; set; }
    public string? EmergenciaNombre { get; set; }
    public string? EmergenciaTelefono { get; set; }
}