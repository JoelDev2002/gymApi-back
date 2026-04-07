using GymApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class MembresiaController : ControllerBase
{
  
  private readonly GimnasioDbContext _context;

  public MembresiaController(GimnasioDbContext context)
  {
    _context= context;
  }

  [HttpGet]
  public async Task<IActionResult> ObtenerMembresias()
  {
    var listaMembresias=await _context.Membresias.Select(m=> new
    {
      MembresiaId = m.MembresiaId,
      Nombre = m.Nombre,
      Descripcion = m.Descripcion,
      DuracionDias = m.DuracionDias,
      Precio = m.Precio,
      EsRenovable = m.EsRenovable,
      IsActive = m.IsActive,
      CreatedAt = m.CreatedAt
    }).ToListAsync();

    return Ok(listaMembresias);
  }

  [HttpPost]
  public async Task<IActionResult> CrearMembresia([FromBody] CrearMembresiaRequest crearMembresia)
  {
    var membresiaExiste= await _context.Membresias.AnyAsync(m=>m.Nombre.ToUpper() == crearMembresia.Nombre.ToUpper());
    if(membresiaExiste) return Conflict("este nombre de membresia ya existe");

    var newMembresia= new Membresia
    {
      Nombre       = crearMembresia.Nombre,
      Descripcion  = crearMembresia.Descripcion,
      DuracionDias = crearMembresia.DuracionDias,
      Precio       = crearMembresia.Precio,
      EsRenovable  = crearMembresia.EsRenovable,
      IsActive     = true,
      CreatedAt    = DateTime.Now
    };

    await _context.Membresias.AddAsync(newMembresia);
    await _context.SaveChangesAsync();


    return Created("Membresia creada",new
    {
      MembresiaId    =newMembresia.MembresiaId,
      Nombre         =newMembresia.Nombre,
      Descripcion    =newMembresia.Descripcion,
      DuracionDias   =newMembresia.DuracionDias,
      Precio         =newMembresia.Precio,
      EsRenovable    =newMembresia.EsRenovable,
      IsActive       =newMembresia.IsActive,
    });
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> ActualizarMembresia (int id ,[FromBody] ActualizarMembresiaRequest actualizarMembresia)
  {
    var membresiaExiste= await _context.Membresias.AnyAsync(m=>m.Nombre.ToUpper() == actualizarMembresia.Nombre.ToUpper() && m.MembresiaId != id);
    if(membresiaExiste) return Conflict("esta membresia ya existe");

    var membresiaEncontrada= await _context.Membresias.FirstOrDefaultAsync(m=> m.MembresiaId == id);
    if (membresiaEncontrada is null) return NotFound("membresia no encontrada");

    membresiaEncontrada.Nombre       = actualizarMembresia.Nombre;
    membresiaEncontrada.Descripcion  = actualizarMembresia.Descripcion;
    membresiaEncontrada.DuracionDias = actualizarMembresia.DuracionDias;
    membresiaEncontrada.Precio       = actualizarMembresia.Precio;
    membresiaEncontrada.EsRenovable  = actualizarMembresia.EsRenovable;

    await _context.SaveChangesAsync();

    return Ok(new
    {
      MembresiaId    =membresiaEncontrada.MembresiaId,
      Nombre         =membresiaEncontrada.Nombre,
      Descripcion    =membresiaEncontrada.Descripcion,
      DuracionDias   =membresiaEncontrada.DuracionDias,
      Precio         =membresiaEncontrada.Precio,
      EsRenovable    =membresiaEncontrada.EsRenovable,
      IsActive       =membresiaEncontrada.IsActive,
    });
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> EliminarMembresia (int id)
  {
    var membresiaExiste=await _context.Membresias.FirstOrDefaultAsync(m=>m.MembresiaId == id);
    if(membresiaExiste is null) return NotFound("membresia no encontrada, intente de nuevo");

    membresiaExiste.IsActive=false;

    await _context.SaveChangesAsync();
    
    return NoContent();
  }

}