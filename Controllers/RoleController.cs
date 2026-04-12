using Microsoft.AspNetCore.Mvc;
using GymApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GymApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
  private readonly GimnasioDbContext _context;

  public RoleController(GimnasioDbContext context)
  {
    _context=context;
  }

  [Authorize(Roles ="ADMIN")]
  [HttpGet]
  public async Task<IActionResult> ObtenerListaRoles()
  {
    var listaRoles=await _context.Roles.ToListAsync();
    return Ok(listaRoles);
  }

  [Authorize(Roles ="ADMIN")]
  [HttpGet("{id}")]
  public async Task<IActionResult> ObtenerRole(int id)
  {
    var role=await _context.Roles.FirstOrDefaultAsync(r=>r.RoleId == id);
    if(role is null ) return NotFound("no se encontro el rol");

    return Ok(role);
  }

  [Authorize(Roles ="ADMIN")]
  [HttpPost]
  public async Task<IActionResult> CrearRole([FromBody] CrearRoleRequest request)
  {
    var roleExiste = await _context.Roles.AnyAsync(r=>r.NormalizedName==request.Name.ToUpper());
    if(roleExiste) return Conflict("ya existe el rol");

    Role newRole=new Role
    {
      Name=request.Name,
      NormalizedName=request.Name.ToUpper(),
      IsActive=true,
      CreatedAt=DateTime.Now
    };
    await _context.Roles.AddAsync(newRole);
    await _context.SaveChangesAsync();

    return Created("creado Correctamente",newRole);
  }

  [Authorize(Roles ="ADMIN")]
  [HttpPut("{id}")]
  public async Task<IActionResult> ActualizarRole(int id,[FromBody] CrearRoleRequest request)
  {
    var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
    if (role == null) return NotFound("rol no existe");

    var existe = await _context.Roles.AnyAsync(r => r.NormalizedName == request.Name.ToUpper() && r.RoleId != id);

    if (existe) return Conflict("ya existe el rol");

    role.Name = request.Name;
    role.NormalizedName = request.Name.ToUpper();

    await _context.SaveChangesAsync();

    return Ok("actualizado correctamente");
  }

  [Authorize(Roles ="ADMIN")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> EliminarRole(int id)
  {
    var roleExiste = await _context.Roles.FirstOrDefaultAsync(r=>r.RoleId==id);
    if(roleExiste is null) return NotFound("no se encontrol el rol, intente de nuevo");

    roleExiste.IsActive=false;
    await _context.SaveChangesAsync();
    return NoContent();
  }
}