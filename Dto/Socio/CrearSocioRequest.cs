using System.ComponentModel.DataAnnotations;

public class CrearSocioRequest
{
    [Required] public string UserName { get; set; }="";
    public string Email { get; set; }="";
    public string Password { get; set; }="";
    public string PhoneNumber { get; set; }="";

    
    public DateOnly? FechaNacimiento { get; set; }
    public string? Genero { get; set; }
    public decimal? AlturaCm { get; set; }
    public decimal? PesoKg { get; set; }
    public string? EmergenciaNombre { get; set; }
    public string? EmergenciaTelefono { get; set; }
}