using System.ComponentModel.DataAnnotations;

public class CompletarSocioRequest
{
    [Required(ErrorMessage ="campo fecha nacimiento obligatorio")]
    public DateOnly? FechaNacimiento { get; set; }
    [Required(ErrorMessage ="campo genero obligatorio")]
    public string? Genero { get; set; }
    [Required(ErrorMessage ="campo altura obligatorio")]
    public decimal? AlturaCm { get; set; }
    [Required(ErrorMessage ="campo peso obligatorio")]
    public decimal? PesoKg { get; set; }
    public string? EmergenciaNombre { get; set; }
    public string? EmergenciaTelefono { get; set; }
}