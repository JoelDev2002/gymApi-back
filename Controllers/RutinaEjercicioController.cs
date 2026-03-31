using GymApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class RutinaEjercicioController : ControllerBase
{ 
  private readonly GimnasioDbContext _context;
  public RutinaEjercicioController(GimnasioDbContext context)
  {
    _context =context;
  }

  [HttpPost("/rutinas/{id}/ejercicio")]
  public async Task<ActionResult> AgregarEjercicio(int id, CrearRutinaEjercicioRequest crearRutinaEjercicio)
  {
    var rutinaExiste = await _context.Rutinas.AnyAsync(r => r.RutinaId == id);
    if (!rutinaExiste) return NotFound("La rutina no existe");

    var ejercicioExiste = await _context.Ejercicios.AnyAsync(e => e.EjercicioId == crearRutinaEjercicio.EjercicioId);
    if (!ejercicioExiste) return BadRequest("El ejercicio no existe");

    var newRutinaEjercicio =new RutinaEjercicio
    {
      RutinaId = id,
      EjercicioId = crearRutinaEjercicio.EjercicioId,
      Orden = crearRutinaEjercicio.Orden,
      Series = crearRutinaEjercicio.Series,
      Repeticiones = crearRutinaEjercicio.Repeticiones,
      PesoObjetivoKg = crearRutinaEjercicio.PesoObjetivoKg,
      DescansoSegundos = crearRutinaEjercicio.DescansoSegundos,
      Notas = crearRutinaEjercicio.Notas
    };

    await _context.RutinaEjercicios.AddAsync(newRutinaEjercicio);
    await _context.SaveChangesAsync();

    return Ok(newRutinaEjercicio);
  }

  [HttpPut("/rutinas/{id}/ejercicio")]
  public async Task<ActionResult> AgregarEjercicio(int id, ActualizarRutinaEjercicioRequest request)
  {
    var rutinaExiste = await _context.Rutinas.AnyAsync(r => r.RutinaId == id);
    if (!rutinaExiste) return NotFound("La rutina no existe");

    var ejercicioExiste = await _context.Ejercicios.AnyAsync(e => e.EjercicioId == request.EjercicioId);
    if (!ejercicioExiste) return BadRequest("El ejercicio no existe");


    RutinaId = id,
    EjercicioId = crearRutinaEjercicio.EjercicioId,
    Orden = crearRutinaEjercicio.Orden,
    Series = crearRutinaEjercicio.Series,
    Repeticiones = crearRutinaEjercicio.Repeticiones,
    PesoObjetivoKg = crearRutinaEjercicio.PesoObjetivoKg,
    DescansoSegundos = crearRutinaEjercicio.DescansoSegundos,
    Notas = crearRutinaEjercicio.Notas
    await _context.SaveChangesAsync();

    return Ok(newRutinaEjercicio);
  }
}