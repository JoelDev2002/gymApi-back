using GymApi.Models;
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



  [HttpGet]
  public async Task<IActionResult> ObtenerRelaciones()
  {
    var listaRelaciones = await _context.SocioEntrenadors
        .Include(sE => sE.Socio)
        .Include(sE => sE.Entrenador)
        .Select(sE => new 
        {
            SocioId          = sE.SocioId,
            EntrenadorId     = sE.EntrenadorId,
            FechaAsignacion  = sE.FechaAsignacion,
            Activo           = sE.Activo,


            SocioNombre      = sE.Socio.User.UserName,
            SocioEmail       = sE.Socio.User.Email,

            EntrenadorNombre = sE.Entrenador.User.UserName,
            EntrenadorEmail  = sE.Entrenador.User.Email
        })
        .ToListAsync();

        return Ok(listaRelaciones);
  }

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

  [HttpPost]
  public async Task<IActionResult> CrearSocioEntrenador([FromBody] CrearSocioEntrenadorRequest crearSocioEntrenador)
  {
    var socioExiste=await _context.Socios.AnyAsync(s=>s.SocioId == crearSocioEntrenador.SocioId);
    if (!socioExiste) return NotFound("Socio no existe");

    var entrenadorExiste=await _context.Entrenadores.AnyAsync(s=>s.EntrenadorId == crearSocioEntrenador.EntrenadorId);
    if (!entrenadorExiste) return NotFound("Entrenador no existe");

    var yaAsignado=await _context.SocioEntrenadors.AnyAsync(sE=> sE.SocioId ==crearSocioEntrenador.SocioId && sE.EntrenadorId == crearSocioEntrenador.EntrenadorId);

    if(yaAsignado) return Conflict("ya esta asignado");

    var newRelacion=new SocioEntrenador
    {
      SocioId=crearSocioEntrenador.SocioId,
      EntrenadorId=crearSocioEntrenador.EntrenadorId,
      FechaAsignacion=DateOnly.FromDateTime(DateTime.Now),
      Activo=true
    };

    await _context.SocioEntrenadors.AddAsync(newRelacion);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(ObtenerRelacion), new { id = newRelacion.SocioId}, newRelacion);
  }

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

  [HttpDelete ("{id}")]
  public async Task<IActionResult> EliminarSocioEntrenador(int id)
  {
    var relacionExiste =await _context.SocioEntrenadors.FirstOrDefaultAsync(sE=>sE.SocioEntrenadorId==id);
    if(relacionExiste is null) return NotFound("relacion no existe");

    relacionExiste.Activo=false;
    return NoContent();
  }
}