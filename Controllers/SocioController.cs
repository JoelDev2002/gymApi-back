using Microsoft.AspNetCore.Mvc;
using GymApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace GymApi.Controller;
[ApiController]
[Route("api/[controller]")]
public class SocioController : ControllerBase
{
  private readonly GimnasioDbContext _context;

  public SocioController(GimnasioDbContext context)
  {
    _context=context;
  }

  [HttpGet]
  public async Task<ActionResult<List<Socio>>> ObtenerSocios()
  {
    List<Socio> listaDeSocios = await _context.Socios.ToListAsync();
    return Ok(listaDeSocios);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> ObtenerSocioPorId(long id)
  {
    var socioEncontrado = await _context.Socios.FirstOrDefaultAsync(s=>s.UserId == id);

    if(socioEncontrado is null) return NotFound("socio no se encontro, intente de nuevo");

    return Ok(socioEncontrado);
  }

  [HttpPost]
  public async Task<IActionResult> CrearSocio([FromBody] CrearSocioRequest crearSocioDto)
  {
    var userNameExiste = await _context.Users.AnyAsync(u=>u.NormalizedUserName ==crearSocioDto.UserName.ToUpper());
    if (userNameExiste) return Conflict("username ya registrado");

    var emailExiste =await _context.Users.AnyAsync(u=>u.NormalizedEmail ==crearSocioDto.Email.ToUpper());
    if (emailExiste) return Conflict("El email ya está registrado");

    var PhoneNumberExiste = await _context.Users.AnyAsync(u=>u.PhoneNumber ==crearSocioDto.PhoneNumber);
    if (PhoneNumberExiste) return Conflict("el numero ya esta registrado");

    var roleExiste=await _context.Roles.FirstOrDefaultAsync(r=>r.NormalizedName== "SOCIO" && r.IsActive == true);
    if(roleExiste is null) return NotFound("no se encontro el rol");

    
    User newUser =new User
    {
      UserName=crearSocioDto.UserName,
      NormalizedUserName=crearSocioDto.UserName.ToUpper(),
      Email=crearSocioDto.Email,
      NormalizedEmail=crearSocioDto.Email.ToUpper(),
      PhoneNumber=crearSocioDto.PhoneNumber,
      CreatedAt=DateTime.Now,
      IsActive=true,
    };
    var hash =new PasswordHasher<User>();
    newUser.PasswordHash=hash.HashPassword(newUser,crearSocioDto.Password);

    newUser.UserRoles.Add(new UserRole
    {
      RoleId=roleExiste.RoleId,
      AssignedAt=DateTime.Now
    });

    newUser.Socio=new Socio
    {
      FechaNacimiento=crearSocioDto.FechaNacimiento,
      Genero=crearSocioDto.Genero,
      AlturaCm=crearSocioDto.AlturaCm,
      PesoKg=crearSocioDto.PesoKg,
      EmergenciaNombre=crearSocioDto.EmergenciaNombre,
      EmergenciaTelefono=crearSocioDto.EmergenciaTelefono,
      FechaRegistro=DateOnly.FromDateTime(DateTime.Now),
      IsActive=true
    };

    await _context.Users.AddAsync(newUser);
    await _context.SaveChangesAsync(); 

    return Created("Creado Exitosamente", new
    {
      UserId=newUser.Socio.UserId,
      Genero=newUser.Socio.Genero,
      AlturaCm=newUser.Socio.AlturaCm,
      PesoKg=newUser.Socio.PesoKg,
      FechaNacimiento=newUser.Socio.FechaNacimiento,
    });
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> EditarSocio(int id , [FromBody] EditarSocioRequest editarSocioRequest)
  {
    var socioExiste=await _context.Socios.FirstOrDefaultAsync(s=>s.UserId==id);

    if(socioExiste is null) return NotFound("socio no existe,intente de nuevo");

    socioExiste.FechaNacimiento=editarSocioRequest.FechaNacimiento;
    socioExiste.Genero=editarSocioRequest.Genero;
    socioExiste.AlturaCm=editarSocioRequest.AlturaCm;
    socioExiste.PesoKg=editarSocioRequest.PesoKg;
    socioExiste.EmergenciaNombre=editarSocioRequest.EmergenciaNombre;
    socioExiste.EmergenciaTelefono=editarSocioRequest.EmergenciaTelefono;

    await _context.SaveChangesAsync();

    return Ok("Actualizado correctamente");
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> EliminarSocio(int id )
  {
    var socioExiste=await _context.Socios.FirstOrDefaultAsync(s=>s.UserId==id);

    if(socioExiste is null) return NotFound("socio no existe,intente de nuevo");

    socioExiste.IsActive=true;
    await _context.SaveChangesAsync();

    return NoContent();
  }

}