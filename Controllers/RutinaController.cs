using Microsoft.AspNetCore.Mvc;
using GymApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

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

  [Authorize(Roles ="ADMIN")]
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

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
[HttpGet("entrenador")]
public async Task<ActionResult> ObtenerRutinasEntrenador()
{
    var query = _context.Rutinas.AsQueryable();

    if (User.IsInRole("ENTRENADOR"))
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        var entrenador = await _context.Entrenadores
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (entrenador is null)
            return NotFound("Entrenador no encontrado");

        query = query.Where(r => r.EntrenadorId == entrenador.EntrenadorId);
    }

    var listaRutinas = await query
        .Select(r => new
        {
            RutinaId = r.RutinaId,
            Socio = new
            {
                SocioId = r.Socio.SocioId,
                Nombre = r.Socio.User.UserName
            },
            Entrenador = new
            {
                EntrenadorId = r.Entrenador!.EntrenadorId,
                Nombre = r.Entrenador.User.UserName
            },
            Nombre = r.Nombre,
            FechaInicio = r.FechaInicio,
            FechaFin = r.FechaFin,
            Activa = r.Activa,
        })
        .ToListAsync();

    return Ok(listaRutinas);
}

  [Authorize(Roles ="ADMIN")]
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

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpPut("{id}")]
  public async Task<IActionResult> EditarRutina(int id , [FromBody] EditarSocioRequest editarSocioRequest)
  {
    
    return null!;
  }

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> EliminarRutina(int id )
  {
    var rutinaExiste= await _context.Rutinas.FirstOrDefaultAsync(r=>r.RutinaId== id);
    if (rutinaExiste is null) return NotFound("rutina no existe");

    rutinaExiste.Activa=false;

    await _context.SaveChangesAsync();

    return NoContent();
  }

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpGet("misrutinas")]
  public async Task<IActionResult> ObtenerMisRutinasEntrenador()
  {
      var userId = int.Parse(User.FindFirst("userId")!.Value);

      var entrenador = await _context.Entrenadores
          .FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive);

      if (entrenador is null)
          return NotFound("entrenador no encontrado");

      var rutinas = await _context.Rutinas
          .Where(r => r.EntrenadorId == entrenador.EntrenadorId && r.Activa)
          .Select(r => new
          {
              RutinaId = r.RutinaId,
              Socio = new
              {
                  SocioId = r.Socio.SocioId,
                  Nombre = r.Socio.User.UserName
              },
              Entrenador = new
              {
                  EntrenadorId = r.Entrenador!.EntrenadorId,
                  Nombre = r.Entrenador.User.UserName
              },
              Nombre = r.Nombre,
              Objetivo = r.Objetivo,
              FechaInicio = r.FechaInicio,
              FechaFin = r.FechaFin,
              Activa = r.Activa
          })
          .ToListAsync();

      return Ok(rutinas);
  }

  [Authorize(Roles ="ADMIN,SOCIO")]
  [HttpGet("socio/misrutinas")]
  public async Task<IActionResult> ObtenerMisRutinasSocio()
  {
      var userId = int.Parse(User.FindFirst("userId")!.Value);

      var socio = await _context.Socios
          .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

      if (socio is null)
          return NotFound("socio no encontrado");

      var rutinas = await _context.Rutinas
          .Where(r => r.SocioId == socio.SocioId && r.Activa)
          .Select(r => new
          {
              RutinaId = r.RutinaId,
              Socio = new
              {
                  SocioId = r.Socio.SocioId,
                  Nombre = r.Socio.User.UserName
              },
              Entrenador = new
              {
                  EntrenadorId = r.Entrenador!.EntrenadorId,
                  Nombre = r.Entrenador.User.UserName
              },
              Nombre = r.Nombre,
              Objetivo = r.Objetivo,
              FechaInicio = r.FechaInicio,
              FechaFin = r.FechaFin,
              Activa = r.Activa
          })
          .ToListAsync();

      return Ok(rutinas);
  }
  }