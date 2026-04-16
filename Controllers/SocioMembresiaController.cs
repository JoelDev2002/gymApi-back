using GymApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class SocioMembresiaController : ControllerBase
{
  private readonly GimnasioDbContext _context;

  public SocioMembresiaController(GimnasioDbContext context)
  {
    _context= context;
  }

    [Authorize(Roles ="ADMIN")]
    [HttpGet]
    public async Task<IActionResult> ListaDeSocioConMembresia()
  {
    var listaDeSocioConMembresia= await _context.SocioMembresia
                                                .Include(sM=>sM.Socio)
                                                .ThenInclude(s=>s.User)
                                                .Include(sM=>sM.Membresia)
                                                .Select(sM => new
                                                {
                                                  SocioMembresiaId=sM.SocioMembresiaId,
                                                  SocioId=sM.SocioId,
  
                                                  SocioNombre=sM.Socio.User.UserName,
                                                  MembresiaId=sM.MembresiaId,
  
                                                  MembresiaNombre=sM.Membresia.Nombre,
                                                  FechaInicio=sM.FechaInicio,
                                                  FechaFin=sM.FechaFin,
  
                                                  Estado=sM.Estado,
  
                                                  MontoPagado=sM.MontoPagado,
  
                                                  Notas=sM.Notas,
                                                })
                                                .ToListAsync();

    return Ok(listaDeSocioConMembresia);
  }

    [Authorize(Roles ="ADMIN")]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerSocioMembresia(int id)
    {
        var socioMembresia = await _context.SocioMembresia
            .Include(sm => sm.Socio)
            .ThenInclude(s => s.User)
            .Include(sm => sm.Membresia)
            .FirstOrDefaultAsync(sm => sm.SocioMembresiaId == id);

        if (socioMembresia is null) return NotFound("membresía no encontrada");

        return Ok(new
        {
            socioMembresia.SocioMembresiaId,
            socioMembresia.SocioId,
            SocioNombre = socioMembresia.Socio.User.UserName,
            socioMembresia.MembresiaId,
            MembresiaNombre = socioMembresia.Membresia.Nombre,
            socioMembresia.FechaInicio,
            socioMembresia.FechaFin,
            socioMembresia.Estado,
            socioMembresia.MontoPagado,
            socioMembresia.Notas
        });
    }

    [Authorize(Roles ="ADMIN")]
    [HttpPost]
    public async Task<IActionResult> AsignarMembresia([FromBody] CrearSocioMembresiaRequest request)
    {
        var socioExiste = await _context.Socios.AnyAsync(s => s.SocioId == request.SocioId && s.IsActive);
        if (!socioExiste) return NotFound("socio no encontrado");

        var membresia = await _context.Membresias.FirstOrDefaultAsync(m => m.MembresiaId == request.MembresiaId && m.IsActive);
        if (membresia is null) return NotFound("membresía no encontrada");

        var tieneActiva = await _context.SocioMembresia.AnyAsync(sm => sm.SocioId == request.SocioId && sm.Estado == "ACTIVA");
        if (tieneActiva) return Conflict("el socio ya tiene una membresía activa");


        var fechaInicio = DateOnly.FromDateTime(DateTime.Today);

        var nuevaMembresia = new SocioMembresium
        {
            SocioId = request.SocioId,
            MembresiaId = request.MembresiaId,
            FechaInicio = fechaInicio,
            FechaFin = fechaInicio.AddDays(membresia.DuracionDias),
            Estado = "ACTIVA",
            MontoPagado = membresia.Precio,
            Notas = request.Notas,
            CreatedAt = DateTime.Now
        };

        await _context.SocioMembresia.AddAsync(nuevaMembresia);
        await _context.SaveChangesAsync();

        return Created("", new
            {
                nuevaMembresia.SocioMembresiaId,
                nuevaMembresia.SocioId,
                nuevaMembresia.MembresiaId,
                MembresiaNombre = membresia.Nombre,
                nuevaMembresia.FechaInicio,
                nuevaMembresia.FechaFin,
                nuevaMembresia.Estado,
                nuevaMembresia.MontoPagado
            });
    }

    [Authorize(Roles ="ADMIN")]
    [HttpPatch("{id}/cancelar")]
    public async Task<IActionResult> CancelarMembresia(int id)
    {
        var socioMembresia = await _context.SocioMembresia.FirstOrDefaultAsync(sm => sm.SocioMembresiaId == id);
        if (socioMembresia is null) return NotFound("membresía no encontrada");
        if (socioMembresia.Estado == "CANCELADA") return Conflict("la membresía ya está cancelada");

        socioMembresia.Estado = "CANCELADA";
        await _context.SaveChangesAsync();

        return Ok(new {
            mensaje = "membresía cancelada",
            socioMembresia.SocioMembresiaId 
          });
    }
}