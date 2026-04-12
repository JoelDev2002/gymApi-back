using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class SocioEntrenadorController : ControllerBase
{
  private readonly GimnasioDbContext _context;

  public SocioEntrenadorController(GimnasioDbContext context)
  {
    _context = context;
  }


  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpGet]
  public async Task<IActionResult> ObtenerRelaciones()
  {
    var listaRelaciones = await _context.SocioEntrenadors
        .Include(sE => sE.Socio)
        .Include(sE => sE.Entrenador)
        .Select(sE => new 
        {
            SocioEntrenadorId  =sE.SocioEntrenadorId,
            SocioId          = sE.SocioId,
            EntrenadorId     = sE.EntrenadorId,
            FechaAsignacion  = sE.FechaAsignacion,
            Activo           = sE.Activo,


            SocioNombre      = sE.Socio.User.UserName,
            SocioEmail       = sE.Socio.User.Email,

            EntrenadorNombre = sE.Entrenador.User.UserName,
            EntrenadorEmail  = sE.Entrenador.User.Email,

            IsActive         =sE.Activo
        })
        .ToListAsync();

        return Ok(listaRelaciones);
  }


[Authorize(Roles ="ADMIN,ENTRENADOR")]
[HttpGet("entrenador")]
public async Task<IActionResult> ObtenerRelacionesEntrenador()
{
    var userId = int.Parse(User.FindFirst("userId")!.Value);

    // Buscar el entrenador por userId
    var entrenador = await _context.Entrenadores
        .FirstOrDefaultAsync(e => e.UserId == userId);

    if (entrenador is null)
        return NotFound("Entrenador no encontrado");

    var listaRelaciones = await _context.SocioEntrenadors
        .Where(sE => sE.EntrenadorId == entrenador.EntrenadorId) 
        .Include(sE => sE.Socio)
        .Include(sE => sE.Entrenador)
        .Select(sE => new 
        {
            SocioEntrenadorId = sE.SocioEntrenadorId,
            SocioId = sE.SocioId,
            EntrenadorId = sE.EntrenadorId,
            FechaAsignacion = sE.FechaAsignacion,
            Activo = sE.Activo,

            SocioNombre = sE.Socio.User.UserName,
            SocioEmail = sE.Socio.User.Email,

            EntrenadorNombre = sE.Entrenador.User.UserName,
            EntrenadorEmail = sE.Entrenador.User.Email,

            IsActive = sE.Activo
        })
        .ToListAsync();

    return Ok(listaRelaciones);
}

  [Authorize(Roles ="ADMIN,ENTRENADOR")]
  [HttpGet("{id}")]
  public async Task<IActionResult> ObtenerRelacion(int id)
  {
    var relacionExiste=await _context.SocioEntrenadors
        .Include(sE => sE.Socio).ThenInclude(s => s.User)
        .Include(sE => sE.Entrenador).ThenInclude(e => e.User)
        .FirstOrDefaultAsync(sE => sE.SocioEntrenadorId == id);

    if  (relacionExiste is null) return NotFound("relacion no encontrada");

    var detalle =new
    {
            SocioId          = relacionExiste.SocioId,
            EntrenadorId     = relacionExiste.EntrenadorId,
            FechaAsignacion  = relacionExiste.FechaAsignacion,
            Activo           = relacionExiste.Activo,


            SocioNombre      = relacionExiste.Socio.User.UserName,
            SocioEmail       = relacionExiste.Socio.User.Email,
            SocioNacimiento  = relacionExiste.Socio.FechaNacimiento,
            SocioActivo      = relacionExiste.Socio.IsActive,
            SocioCreado      = relacionExiste.Socio.FechaRegistro,

            EntrenadorNombre = relacionExiste.Entrenador.User.UserName,
            EntrenadorEmail  = relacionExiste.Entrenador.User.Email,
            EntrenadorActivo = relacionExiste.Entrenador.IsActive,
            EntrenadorCreado = relacionExiste.Entrenador.FechaIngreso,
    };

    return Ok(detalle);
  }

  [Authorize(Roles ="ADMIN")]
  [HttpPost]
public async Task<IActionResult> CrearSocioEntrenador([FromBody] CrearSocioEntrenadorRequest crearSocioEntrenador)
{
    var socioExiste = await _context.Socios
        .AnyAsync(s => s.SocioId == crearSocioEntrenador.SocioId && s.IsActive);

    if (!socioExiste) return NotFound("Socio no existe");

    var entrenadorExiste = await _context.Entrenadores
        .AnyAsync(e => e.EntrenadorId == crearSocioEntrenador.EntrenadorId && e.IsActive);

    if (!entrenadorExiste) return NotFound("Entrenador no existe");

    var socioYaTieneEntrenador = await _context.SocioEntrenadors
        .AnyAsync(se => se.SocioId == crearSocioEntrenador.SocioId && se.Activo);

    if (socioYaTieneEntrenador)
        return Conflict("El socio ya tiene un entrenador asignado");

    var newRelacion = new SocioEntrenador
    {
        SocioId = crearSocioEntrenador.SocioId,
        EntrenadorId = crearSocioEntrenador.EntrenadorId,
        FechaAsignacion = DateOnly.FromDateTime(DateTime.Now),
        Activo = true
    };

    await _context.SocioEntrenadors.AddAsync(newRelacion);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(ObtenerRelacion), new { id = newRelacion.SocioEntrenadorId }, newRelacion);
}

  [Authorize(Roles ="ADMIN")]
  [HttpPut ("{id}")]
  public async Task<IActionResult> ActualizarSocioEntrenador(int id, [FromBody] ActualizarSocioEntrenadorRequest actualizarSocioEntrenadorRequest )
  {
    var socioExiste=await _context.Socios.AnyAsync(s=>s.SocioId == actualizarSocioEntrenadorRequest.SocioId);
    if (!socioExiste) return NotFound("Socio no existe");

    var entrenadorExiste=await _context.Entrenadores.AnyAsync(s=>s.EntrenadorId == actualizarSocioEntrenadorRequest.EntrenadorId);
    if (!entrenadorExiste) return NotFound("Entrenador no existe");

    var relacionExiste =await _context.SocioEntrenadors.FirstOrDefaultAsync(sE=>sE.SocioEntrenadorId==id);
    if(relacionExiste is null) return NotFound("relacion no existe");

    var newRelacionExiste= await _context.SocioEntrenadors.AnyAsync(sE=>sE.EntrenadorId==actualizarSocioEntrenadorRequest.EntrenadorId && sE.SocioId ==actualizarSocioEntrenadorRequest.SocioId && sE.Activo &&sE.SocioEntrenadorId !=id);
    if(newRelacionExiste) return Conflict("ya existe esta relacion entre socio y entrenador");

    relacionExiste.SocioId=actualizarSocioEntrenadorRequest.SocioId;
    relacionExiste.EntrenadorId=actualizarSocioEntrenadorRequest.EntrenadorId;

    await _context.SaveChangesAsync();

    return Ok(new
    {
      SocioId=relacionExiste.SocioId,
      EntrenadorId=relacionExiste.EntrenadorId,
    });
  }

  [Authorize(Roles ="ADMIN")]
  [HttpDelete ("{id}")]
  public async Task<IActionResult> EliminarSocioEntrenador(int id)
  {
    var relacionExiste =await _context.SocioEntrenadors.FirstOrDefaultAsync(sE=>sE.SocioEntrenadorId==id);
    if(relacionExiste is null) return NotFound("relacion no existe");

    relacionExiste.Activo=false;
    await _context.SaveChangesAsync();
    return NoContent();
  }
}