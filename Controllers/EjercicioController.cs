using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EjercicioController : ControllerBase
{
  private readonly GimnasioDbContext _context;

  public EjercicioController(GimnasioDbContext context)
  {
    _context=context;
  }

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpGet]
  public async Task<IActionResult> ObtenerListajercicios()
  {
    var listaEjercicio=await _context.Ejercicios.Select(e => new
    {
      EjercicioId=e.EjercicioId,
      Nombre=e.Nombre,
      Descripcion=e.Descripcion,
      GrupoMuscular=e.GrupoMuscular,
      IsActive=e.IsActive,
    }).ToListAsync();
    return Ok(listaEjercicio);
  }

  [Authorize(Roles ="ADMIN,ENTRENADOR,SOCIO")]
  [HttpGet("{id}")]
  public async Task<IActionResult> ObtenerEjercicio(int id)
  {
    var ejercicio=await _context.Ejercicios.FirstOrDefaultAsync(e=>e.EjercicioId == id);
    if(ejercicio is null ) return NotFound("no se encontro el rol");


    return Ok(new
    {
      EjercicioId=ejercicio.EjercicioId,
      Nombre=ejercicio.Nombre,
      Descripcion=ejercicio.Descripcion,
      GrupoMuscular=ejercicio.GrupoMuscular,
      IsActive=ejercicio.IsActive,
    });
  }

  [Authorize(Roles ="ADMIN")]
  [HttpPost]
  public async Task<IActionResult> CrearEjercicio([FromBody] CrearEjercicioRequest request)
  {
    var ejercicioExiste = await _context.Ejercicios.AnyAsync(e=>e.Nombre.ToUpper()==request.Nombre.ToUpper());
    if(ejercicioExiste) return Conflict("ya existe este ejercicio");

    var newEjercicio=new Ejercicio
    {
      Nombre=request.Nombre,
      Descripcion=request.Descripcion,
      GrupoMuscular=request.GrupoMuscular,
      IsActive=true,
    };
    await _context.Ejercicios.AddAsync(newEjercicio);
    await _context.SaveChangesAsync();

    return Created("",new
    {
      EjercicioId=newEjercicio.EjercicioId,
      Nombre=newEjercicio.Nombre,
      Descripcion=newEjercicio.Descripcion,
      GrupoMuscular=newEjercicio.GrupoMuscular,
      IsActive=newEjercicio.IsActive,
    });
  }

  [Authorize(Roles ="ADMIN")]
  [HttpPut]
  public async Task<IActionResult> ActualizarEjercicio(int id,[FromBody] ActualizarEjercicioRequest request)
  {
    var ejercicioExiste = await _context.Ejercicios.FirstOrDefaultAsync(e => e.EjercicioId == id);
    if (ejercicioExiste is null) return NotFound("Ejercicio no existe");

    var existe = await _context.Ejercicios.AnyAsync(e => e.Nombre.ToUpper() == request.Nombre.ToUpper() && e.EjercicioId != id);

    if (existe) return Conflict("ya existe el ejercicio");

    ejercicioExiste.Nombre = request.Nombre;
    ejercicioExiste.Descripcion = request.Descripcion;
    ejercicioExiste.GrupoMuscular = request.GrupoMuscular;

    await _context.SaveChangesAsync();

    return Ok("actualizado correctamente");
  }

  [Authorize(Roles ="ADMIN")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> EliminarEjercicio(int id)
  {
    var ejercicioExiste = await _context.Ejercicios.FirstOrDefaultAsync(e=>e.EjercicioId == id);
    if(ejercicioExiste is null) return NotFound("no se encontrol el rol, intente de nuevo");

    ejercicioExiste.IsActive=false;
    await _context.SaveChangesAsync();
    return NoContent();
  }
}