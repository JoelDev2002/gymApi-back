using Microsoft.AspNetCore.Mvc;
using GymApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Azure;
using Microsoft.AspNetCore.Authorization;

namespace GymApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
  private readonly GimnasioDbContext _context;

  public UserController(GimnasioDbContext context)
  {
    _context=context;
  }

  [Authorize(Roles ="ADMIN")]
  [HttpGet]
  public async Task<IActionResult> ObtenerListaUsers()
  {
    var listaUsers = await _context.Users.Include(u=>u.UserRoles).ThenInclude(uR=>uR.Role)
    .Select(u => new UserResponse
    {
      UserId = u.UserId,
      UserName = u.UserName,
      Email = u.Email,
      PhoneNumber = u.PhoneNumber ?? "",
      IsActive = u.IsActive,
      Roles = u.UserRoles.Select(uR=>uR.Role.Name).ToList()
    }).ToListAsync();

    return Ok(listaUsers);
  }

  [Authorize(Roles ="ADMIN,ENTRENADOR,SOCIO")]
  [HttpGet("{id}")]
  public async Task<IActionResult> ObtenerUser(int id)
  {
    var userEncontrado=await _context.Users.Include(u => u.UserRoles).ThenInclude(uR=>uR.Role).FirstOrDefaultAsync(u=>u.UserId == id);

    if (userEncontrado is null) return NotFound("usuario no encontrado");

    var userResponse=new UserResponse
    {
      UserId = userEncontrado.UserId,
      UserName = userEncontrado.UserName,
      Email = userEncontrado.Email,
      PhoneNumber = userEncontrado.PhoneNumber ?? "",
      IsActive = userEncontrado.IsActive,
      Roles = userEncontrado.UserRoles.Select(uR=>uR.Role.Name).ToList()
    };

    return Ok(userResponse);
  }

  [Authorize(Roles ="ADMIN")]
  [HttpPost]
  public async Task<IActionResult> CrearUser([FromBody] CrearUserRequest crearUserRequest)
  {
    var userNameExiste = await _context.Users.AnyAsync(u=>u.NormalizedUserName ==crearUserRequest.UserName.ToUpper());
    if (userNameExiste) return Conflict("username ya registrado");

    var emailExiste =await _context.Users.AnyAsync(u=>u.NormalizedEmail ==crearUserRequest.Email.ToUpper());
    if (emailExiste) return Conflict("El email ya está registrado");

    var PhoneNumberExiste = await _context.Users.AnyAsync(u=>u.PhoneNumber ==crearUserRequest.PhoneNumber);
    if (PhoneNumberExiste) return Conflict("el numero ya esta registrado");

    List<Role> rolesExiste= await _context.Roles
    .Where(r=> crearUserRequest.Roles.Contains(r.RoleId) && r.IsActive == true).ToListAsync();

    
    User newUser =new User
    {
      UserName=crearUserRequest.UserName,
      NormalizedUserName=crearUserRequest.UserName.ToUpper(),
      Email=crearUserRequest.Email,
      NormalizedEmail=crearUserRequest.Email.ToUpper(),
      PhoneNumber=crearUserRequest.PhoneNumber,
      CreatedAt=DateTime.Now,
      IsActive=true,
    };
    var hash =new PasswordHasher<User>();
    newUser.PasswordHash=hash.HashPassword(newUser,crearUserRequest.Password);

    foreach (var roles in rolesExiste)
    {
      newUser.UserRoles.Add(new UserRole
      {
        RoleId=roles.RoleId,
        AssignedAt=DateTime.Now
      });
    }

    await _context.Users.AddAsync(newUser);
    await _context.SaveChangesAsync();

    return Created("creado de manera existosa",new 
    {
        newUser.UserId,
        newUser.UserName,
        newUser.Email,
        newUser.PhoneNumber
    });
  }

  [Authorize(Roles ="ADMIN")]
  [HttpPut("{id}")]
  public async Task<IActionResult> ActualizarUser(int id ,[FromBody] ActualizarUserRequest actualizarUserRequest)
  {
    var userExiste=await _context.Users.FirstOrDefaultAsync(u=>u.UserId==id);
    if(userExiste is null ) return NotFound("usuario no encontrado");

    var userNameExiste= await _context.Users.AnyAsync(u=>u.NormalizedUserName == actualizarUserRequest.UserName.ToUpper() && u.UserId != id);
    if(userNameExiste)return Conflict("ya esta en uso este nombre");

    var PhoneNumberExiste = await _context.Users.AnyAsync(u=>u.PhoneNumber ==actualizarUserRequest.PhoneNumber && u.UserId != id);
    if (PhoneNumberExiste) return Conflict("el numero ya esta registrado");

    userExiste.UserName=actualizarUserRequest.UserName;
    userExiste.NormalizedUserName=actualizarUserRequest.UserName.ToUpper();
    userExiste.PhoneNumber=actualizarUserRequest.PhoneNumber;

    if (!string.IsNullOrEmpty(actualizarUserRequest.Password))
    {
    var hasher = new PasswordHasher<User>();
    userExiste.PasswordHash = hasher.HashPassword(userExiste, actualizarUserRequest.Password);
    }

    userExiste.UpdatedAt=DateTime.Now;
    await _context.SaveChangesAsync();
    return Ok("actualizado");
  }

  [Authorize(Roles ="ADMIN")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> eliminarUser(int id)
  {
    var userExiste=await _context.Users.FirstOrDefaultAsync(u=>u.UserId==id);
    if(userExiste is null ) return NotFound("usuario no encontrado");

    userExiste.IsActive=false;

    await _context.SaveChangesAsync();
    return NoContent();
  }
}