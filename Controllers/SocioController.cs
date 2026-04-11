using Microsoft.AspNetCore.Mvc;
using GymApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

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

  [Authorize(Roles ="ADMIN")]
  [HttpGet]
  public async Task<ActionResult> ObtenerSocios()
  {
    var listaDeSocios = await _context.Socios
                                              .Include(s=>s.User)
                                              .Select(s=> new
                                              {
                                                UserId=s.UserId,
                                                SocioId=s.SocioId,
                                                SocioNombre=s.User !=null?s.User.UserName:"Sin Usuario",
                                                Genero=s.Genero,
                                                Altura=s.AlturaCm,
                                                Peso=s.PesoKg,
                                                IsActive=s.IsActive
                                              })
                                              .ToListAsync();
    return Ok(listaDeSocios);
  }

  [Authorize(Roles ="ADMIN,SOCIO")]
  [HttpGet("{id}")]
  public async Task<IActionResult> ObtenerSocioPorId(int id)
  {
    var socioEncontrado = await _context.Socios.Include(s=>s.User).FirstOrDefaultAsync(s=>s.SocioId == id);

    if(socioEncontrado is null) return NotFound("socio no se encontro, intente de nuevo");

    return Ok(new
    {
      socioEncontrado.SocioId,
      socioEncontrado.User.UserName,
      socioEncontrado.User.Email,
      socioEncontrado.PesoKg,
      socioEncontrado.AlturaCm,
      socioEncontrado.FechaNacimiento,
      socioEncontrado.Genero,
      socioEncontrado.EmergenciaNombre,
      socioEncontrado.EmergenciaTelefono
    });
  }
  
  [Authorize(Roles ="SOCIO")]
  [HttpGet("miperfil")]
  public async Task<IActionResult> ObtenerMiPerfil()
  {
    var userId = int.Parse(User.FindFirst("userId")!.Value);

    var socioEncontrado= await _context.Socios
                                        .Include(s=>s.User)
                                        .FirstOrDefaultAsync(s=>s.UserId == userId);

    if(socioEncontrado is null)
    {
      return NotFound(new
      {
        TienePerfil=false,
        Message="Perfil incompleto , completa tus datos"
      });
    }
    return Ok(new
    {
      TienePerfil=true,
      socioEncontrado.SocioId,
      socioEncontrado.User.UserName,
      socioEncontrado.PesoKg,
      socioEncontrado.AlturaCm,
      socioEncontrado.FechaNacimiento,
      socioEncontrado.Genero,
      socioEncontrado.EmergenciaNombre,
      socioEncontrado.EmergenciaTelefono
    });
  }
  
  [Authorize(Roles ="ADMIN")]
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

  [Authorize(Roles ="ADMIN,SOCIO")]
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

  [Authorize(Roles ="ADMIN")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> EliminarSocio(int id )
  {
    var socioExiste=await _context.Socios.FirstOrDefaultAsync(s=>s.UserId==id);

    if(socioExiste is null) return NotFound("socio no existe,intente de nuevo");

    socioExiste.IsActive=false;
    await _context.SaveChangesAsync();

    return NoContent();
  }

  [Authorize(Roles ="ADMIN,SOCIO")]
  [HttpPost("completarsocio")]
  public async Task<IActionResult> CompletarSocio([FromBody] CompletarSocioRequest completarSocio)
  {
    var userId = int.Parse(User.FindFirst("userId")!.Value);

    var userExiste=await _context.Users.AnyAsync(u=>u.UserId == userId);
    if(!userExiste) return NotFound("no se encontro este usuario");

    var socioExiste = await _context.Socios.AnyAsync(s => s.UserId == userId);
    if (socioExiste) return Conflict("este usuario ya tiene un perfil de socio");

    var newSocio = new Socio
    {
      UserId             = userId,
      FechaNacimiento    = completarSocio.FechaNacimiento,
      Genero             = completarSocio.Genero,
      AlturaCm           = completarSocio.AlturaCm,
      PesoKg             = completarSocio.PesoKg,
      EmergenciaNombre   = completarSocio.EmergenciaNombre,
      EmergenciaTelefono = completarSocio.EmergenciaTelefono,
      FechaRegistro      = DateOnly.FromDateTime(DateTime.Now),
      IsActive           = true
    };

    await _context.Socios.AddAsync(newSocio);
    await _context.SaveChangesAsync();

    return Created("creado correctamente", newSocio);
  }
}