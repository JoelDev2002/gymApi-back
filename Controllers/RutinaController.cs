using Microsoft.AspNetCore.Mvc;
using GymApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace GymApi.Controller;
[ApiController]
[Route("api/[controller]")]
public class RutinaController : ControllerBase
{
  private readonly GimnasioDbContext _context;

  public RutinaController(GimnasioDbContext context)
  {
    _context=context;
  }

  [HttpGet]
  public async Task<ActionResult> ObtenerRutinas()
  {
    var listaRutinas = await _context.Rutinas
                                      .Select(r => new
                                      {
                                        RutinaId=r.RutinaId,
                                        Socio = new
                                        {
                                          SocioId=r.Socio.SocioId,
                                          Nombre=r.Socio.User.UserName
                                        },
                                        Entrenador = new
                                        {
                                          EntreadorId=r.Entrenador!.EntrenadorId,
                                          Nombre=r.Entrenador.User.UserName
                                        },
                                        Nombre=r.Nombre,
                                        FechaInicio=r.FechaInicio,
                                        FechaFin=r.FechaFin,
                                        Activa=r.Activa,
                                      }).ToListAsync();

    return Ok(listaRutinas);
  }


  [HttpPost]
  public async Task<IActionResult> CrearRutina([FromBody] CrearRutinaRequest crearRutina)
  {
    var socioExiste= await _context.Socios.AnyAsync(s=>s.SocioId== crearRutina.SocioId);
    if (!socioExiste) return NotFound("socio no existe");

    var entrenadorExiste= await _context.Entrenadores.AnyAsync(r=>r.EntrenadorId== crearRutina.EntrenadorId);
    if (!entrenadorExiste) return NotFound("entrenador no existe");

    var rutinaExiste = await _context.Rutinas.AnyAsync(r =>
    r.SocioId == crearRutina.SocioId &&
    r.EntrenadorId == crearRutina.EntrenadorId &&
    r.Activa &&
    (
        crearRutina.FechaInicio <= r.FechaFin &&
        crearRutina.FechaFin >= r.FechaInicio
    )
    );

    if (rutinaExiste) return BadRequest("ya existe una rutina en ese rango de fecha y socio con entrenador");

    var newRutina =new Rutina
    {
      SocioId=crearRutina.SocioId,
      EntrenadorId=crearRutina.EntrenadorId,
      Nombre= crearRutina.Nombre,
      Objetivo=crearRutina.Objetivo,
      FechaInicio=crearRutina.FechaInicio,
      FechaFin=crearRutina.FechaFin,
      Activa=true,
      CreatedAt=DateTime.Now
    };

    await _context.Rutinas.AddAsync(newRutina);
    await _context.SaveChangesAsync();

    return Created("rutina creada",new
    {
      RutinaId=newRutina.RutinaId,
      SocioId=newRutina.SocioId,
      EntrenadorId=newRutina.EntrenadorId,
      Nombre= newRutina.Nombre,
      Objetivo=newRutina.Objetivo,
      FechaInicio=newRutina.FechaInicio,
      FechaFin=newRutina.FechaFin,
      Activa=true,
      CreatedAt=DateTime.Now
    });
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> EditarRutina(int id , [FromBody] EditarSocioRequest editarSocioRequest)
  {
    
    return null!;
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> EliminarRutina(int id )
  {
    var rutinaExiste= await _context.Rutinas.FirstOrDefaultAsync(r=>r.RutinaId== id);
    if (rutinaExiste is null) return NotFound("rutina no existe");

    rutinaExiste.Activa=false;

    await _context.SaveChangesAsync();

    return NoContent();
  }
}