using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
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

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpPost("{id}/ejercicio")]
  public async Task<ActionResult> AgregarRutinaEjercicio(int id, CrearRutinaEjercicioRequest crearRutinaEjercicio)
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

    return Created("",newRutinaEjercicio);
  }

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpPut("{rEId}")]
  public async Task<ActionResult> ActualizarRutinaEjercicio(int rEId, ActualizarRutinaEjercicioRequest request)
  {

    var rutinaEjercicio = await _context.RutinaEjercicios.FirstOrDefaultAsync(re => re.RutinaEjercicioId == rEId);
    if (rutinaEjercicio == null) return NotFound("El ejercicio en la rutina no existe");



    rutinaEjercicio.Orden = request.Orden;
    rutinaEjercicio.Series = request.Series;
    rutinaEjercicio.Repeticiones = request.Repeticiones;
    rutinaEjercicio.PesoObjetivoKg = request.PesoObjetivoKg;
    rutinaEjercicio.DuracionSegundos = request.DuracionSegundos;
    rutinaEjercicio.DescansoSegundos = request.DescansoSegundos;
    rutinaEjercicio.Notas = request.Notas;

    await _context.SaveChangesAsync();

    return Ok(rutinaEjercicio);
  }

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpDelete("{rEId}")]
  public async Task<ActionResult> EliminarRutinaEjercicio(int rEId)
  {
      var rutinaEjercicio = await _context.RutinaEjercicios.FirstOrDefaultAsync(re => re.RutinaEjercicioId == rEId);

      if (rutinaEjercicio is null) return NotFound("El ejercicio en la rutina no existe");

      _context.RutinaEjercicios.Remove(rutinaEjercicio);
      await _context.SaveChangesAsync();

      return Ok(new { mensaje = "Ejercicio eliminado de la rutina" });
  }

  [Authorize(Roles ="ADMIN,ENTRENADOR,SOCIO")]
  [HttpGet("{rutinaId}/ejercicios")]
  public async Task<ActionResult> ObtenerEjerciciosPorRutina(int rutinaId)
  {
      var rutinaExiste = await _context.Rutinas.AnyAsync(r => r.RutinaId == rutinaId);
      if (!rutinaExiste) return NotFound("La rutina no existe");

      var ejercicios = await _context.RutinaEjercicios
          .Include(re => re.Ejercicio)
          .Where(re => re.RutinaId == rutinaId)
          .OrderBy(re => re.Orden)
          .Select(re => new
          {
              re.RutinaEjercicioId,
              re.Orden,
              re.Series,
              re.Repeticiones,
              re.PesoObjetivoKg,
              re.DuracionSegundos,
              re.DescansoSegundos,
              re.Notas,
              Ejercicio = new
              {
                  re.Ejercicio.EjercicioId,
                  re.Ejercicio.Nombre,
                  re.Ejercicio.GrupoMuscular
              }
          })
          .ToListAsync();

      return Ok(ejercicios);
  }
}