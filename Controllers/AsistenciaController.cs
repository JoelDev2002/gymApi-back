using GymApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AsistenciaController : ControllerBase
{
  private readonly GimnasioDbContext _context;

    public AsistenciaController(GimnasioDbContext context)
    {
        _context = context;
    }

    [HttpPost("entrada")]
    public async Task<IActionResult> RegistrarEntrada([FromBody] string? observaciones)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        var socio = await _context.Socios.FirstOrDefaultAsync(s => s.UserId == userId);
        if (socio is null) return NotFound("Perfil de socio no encontrado");

        // Verificar que no tenga una entrada abierta (sin salida)
        var entradaAbierta = await _context.Asistencias
            .AnyAsync(a => a.SocioId == socio.SocioId && a.FechaHoraSalida == null);
        if (entradaAbierta) return Conflict("Ya tenés una entrada registrada sin salida");

        var asistencia = new Asistencia
        {
            SocioId            = socio.SocioId,
            FechaHoraEntrada   = DateTime.Now,
            Observaciones      = observaciones,
            RegistradaPorUserId = userId
        };

        await _context.Asistencias.AddAsync(asistencia);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            asistencia.AsistenciaId,
            asistencia.FechaHoraEntrada,
            mensaje = "Entrada registrada correctamente"
        });
    }

    [HttpPut("salida")]
    public async Task<IActionResult> RegistrarSalida()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        var socio = await _context.Socios.FirstOrDefaultAsync(s => s.UserId == userId);
        if (socio is null) return NotFound("Perfil de socio no encontrado");

        var asistencia = await _context.Asistencias
            .FirstOrDefaultAsync(a => a.SocioId == socio.SocioId && a.FechaHoraSalida == null);
        if (asistencia is null) return NotFound("No tenés una entrada activa");

        asistencia.FechaHoraSalida = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            asistencia.AsistenciaId,
            asistencia.FechaHoraEntrada,
            asistencia.FechaHoraSalida,
            Duracion = $"{(asistencia.FechaHoraSalida - asistencia.FechaHoraEntrada)!.Value.TotalMinutes:F0} minutos",
            mensaje = "Salida registrada correctamente"
        });
    }

    // El socio ve su propio historial
    [HttpGet("mihistorial")]
    public async Task<IActionResult> ObtenerMiHistorial()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        var socio = await _context.Socios.FirstOrDefaultAsync(s => s.UserId == userId);
        if (socio is null) return NotFound("Perfil de socio no encontrado");

        var historial = await _context.Asistencias
            .Where(a => a.SocioId == socio.SocioId)
            .OrderByDescending(a => a.FechaHoraEntrada)
            .Select(a => new
            {
                a.AsistenciaId,
                a.FechaHoraEntrada,
                a.FechaHoraSalida,
                a.Observaciones,
                Duracion = a.FechaHoraSalida == null
                    ? "En gimnasio"
                    : $"{(a.FechaHoraSalida.Value - a.FechaHoraEntrada).TotalMinutes:F0} minutos"
            })
            .ToListAsync();

        return Ok(historial);
    }

    [HttpGet("hoy")]
    public async Task<IActionResult> ObtenerAsistenciasHoy()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var asistencias = await _context.Asistencias
            .Include(a => a.Socio).ThenInclude(s => s.User)
            .Where(a => DateOnly.FromDateTime(a.FechaHoraEntrada) == hoy)
            .OrderByDescending(a => a.FechaHoraEntrada)
            .Select(a => new
            {
                a.AsistenciaId,
                Socio = new { a.Socio.SocioId, a.Socio.User.UserName },
                a.FechaHoraEntrada,
                a.FechaHoraSalida,
                Estado = a.FechaHoraSalida == null ? "En gimnasio" : "Salió",
                a.Observaciones
            })
            .ToListAsync();

        return Ok(asistencias);
    }

    [HttpPost("admin/entrada/{socioId}")]
    public async Task<IActionResult> RegistrarEntradaAdmin(int socioId, [FromBody] string? observaciones)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);

        var socioExiste = await _context.Socios.AnyAsync(s => s.SocioId == socioId);
        if (!socioExiste) return NotFound("Socio no encontrado");

        var entradaAbierta = await _context.Asistencias
            .AnyAsync(a => a.SocioId == socioId && a.FechaHoraSalida == null);
        if (entradaAbierta) return Conflict("El socio ya tiene una entrada activa");

        var asistencia = new Asistencia
        {
            SocioId             = socioId,
            FechaHoraEntrada    = DateTime.Now,
            Observaciones       = observaciones,
            RegistradaPorUserId = userId  
        };

        await _context.Asistencias.AddAsync(asistencia);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            asistencia.AsistenciaId,
            asistencia.FechaHoraEntrada,
            mensaje = "Entrada registrada por admin"
        });
    }
}