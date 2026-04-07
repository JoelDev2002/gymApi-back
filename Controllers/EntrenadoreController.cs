using GymApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EntrenadoreController : ControllerBase
{
  private readonly GimnasioDbContext _context;
  
  public EntrenadoreController(GimnasioDbContext context)
  {
    _context=context;
  }

  [HttpGet]
  public async Task<IActionResult> ObtenerEntrenadores()
  {
    var listaEntrenadores= await _context.Entrenadores.ToListAsync();

    return Ok(listaEntrenadores);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> ObtenerEntrenadorPorId(int id)
  {
    var entrenadorExiste= await _context.Entrenadores.FirstOrDefaultAsync(e=>e.UserId ==id);
    if(entrenadorExiste is null) return NotFound("no se encontro al entrenador");

    return Ok(entrenadorExiste);
  }

  [HttpPost]
  public async Task<IActionResult> CrearEntrenador([FromBody] CrearEntrenadoreRequest crearEntrenadore)
  {
    var userNameExiste=await _context.Users.AnyAsync(u=>u.NormalizedUserName == crearEntrenadore.UserName.ToUpper());
    if(userNameExiste)return Conflict("ya existe usuario con este username");

    var emailExiste=await _context.Users.AnyAsync(u=>u.NormalizedEmail == crearEntrenadore.Email.ToUpper());
    if(emailExiste)return Conflict("ya existe usuario con este email");

    var phoneNumberExiste=await _context.Users.AnyAsync(u=>u.PhoneNumber == crearEntrenadore.PhoneNumber);
    if(phoneNumberExiste)return Conflict("ya existe usuario con este numero");

    var roleExiste=await _context.Roles.FirstOrDefaultAsync(r=>r.NormalizedName== "ENTRENADOR" && r.IsActive == true);
    if(roleExiste is null) return NotFound("no se encontro el rol");

    var newUser=new User
    {
      UserName=crearEntrenadore.UserName,
      NormalizedUserName=crearEntrenadore.UserName.ToUpper(),
      Email=crearEntrenadore.Email,
      NormalizedEmail=crearEntrenadore.Email.ToUpper(),
      PhoneNumber=crearEntrenadore.PhoneNumber,
      IsActive=true,
      CreatedAt=DateTime.Now
    };

    var hasher =new PasswordHasher<User>();
    newUser.PasswordHash=hasher.HashPassword(newUser,crearEntrenadore.Password);

    newUser.UserRoles.Add(new UserRole
    {
      RoleId=roleExiste.RoleId,
      AssignedAt=DateTime.Now
    });

    newUser.Entrenadore=new Entrenadore
    {
      Especialidad=crearEntrenadore.Especialidad,
      Certificaciones=crearEntrenadore.Certificaciones,
      FechaIngreso=DateOnly.FromDateTime(DateTime.Now),
      IsActive=true
    };

    await _context.AddAsync(newUser);
    await _context.SaveChangesAsync();

    return Created("creado exitosamente", new
    {
      UserId=newUser.UserId,
      Especialidad=newUser.Entrenadore.Especialidad,
      Certificaciones=newUser.Entrenadore.Certificaciones,
    });
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> ActualizarEntrenador(int id, [FromBody]ActualizarEntrenadoreRequest actualizarEntrenadore)
  {
    var entrenadoreExiste=await _context.Entrenadores.FirstOrDefaultAsync(e=>e.UserId ==id);
    if(entrenadoreExiste is null) return NotFound("entrenador no encontrado");

    entrenadoreExiste.Especialidad=actualizarEntrenadore.Especialidad;
    entrenadoreExiste.Certificaciones=actualizarEntrenadore.Certificaciones;

    await _context.SaveChangesAsync();

    return Created("creado exitosamente", new
    {
      UserId=entrenadoreExiste.UserId,
      Especialidad=entrenadoreExiste.Especialidad,
      Certificaciones=entrenadoreExiste.Certificaciones,
    });
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> EliminarEntrenador(int id)
  {
    var entrenadoreExiste= await _context.Entrenadores.FirstOrDefaultAsync(e=>e.UserId == id);
    if(entrenadoreExiste is null ) return NotFound("entrenador no existe");

    entrenadoreExiste.IsActive=false;

    return NoContent();
  }
}