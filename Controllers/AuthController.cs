using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly GimnasioDbContext _context;
  private readonly IConfiguration _configuration;

  public AuthController(GimnasioDbContext context , IConfiguration configuration)
  {
    _context=context;
    _configuration=configuration;
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
  {
    var userExiste =await _context.Users
                                  .Include(u=>u.UserRoles)
                                  .ThenInclude(uR=>uR.Role)
                                  .FirstOrDefaultAsync(user=>user.NormalizedEmail ==loginRequest.Email.ToUpper());
    if (userExiste is null) return Unauthorized("correo o contraseña incorrecta");

    var hasher=new PasswordHasher<User>();
    var result=hasher.VerifyHashedPassword(userExiste,userExiste.PasswordHash,loginRequest.Password);

    if(result == PasswordVerificationResult.Failed) return Unauthorized("correo o contraseña incorrecta");

    var tokenGenerado=GenerarToken(userExiste!);

    return Ok(new {
        token=tokenGenerado
      });
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
  {
    
    var userNameExiste = await _context.Users.AnyAsync(u=>u.NormalizedUserName ==registerRequest.UserName.ToUpper());
    if (userNameExiste) return Conflict("username ya registrado");

    var emailExiste =await _context.Users.AnyAsync(u=>u.NormalizedEmail ==registerRequest.Email.ToUpper());
    if (emailExiste) return Conflict("El email ya está registrado");

    var PhoneNumberExiste = await _context.Users.AnyAsync(u=>u.PhoneNumber ==registerRequest.PhoneNumber);
    if (PhoneNumberExiste) return Conflict("el numero ya esta registrado");

    var roleExiste=await _context.Roles.FirstOrDefaultAsync(r=>r.NormalizedName== "SOCIO" && r.IsActive == true);
    if(roleExiste is null) return NotFound("no se encontro el rol");

    
    User newUser =new User
    {
      UserName=registerRequest.UserName,
      NormalizedUserName=registerRequest.UserName.ToUpper(),
      Email=registerRequest.Email,
      NormalizedEmail=registerRequest.Email.ToUpper(),
      PhoneNumber=registerRequest.PhoneNumber,
      CreatedAt=DateTime.Now,
      IsActive=true,
    };
    var hash =new PasswordHasher<User>();
    newUser.PasswordHash=hash.HashPassword(newUser,registerRequest.Password);

    newUser.UserRoles.Add(new UserRole
    {
      RoleId=roleExiste.RoleId,
      AssignedAt=DateTime.Now
    });

    await _context.Users.AddAsync(newUser);
    await _context.SaveChangesAsync();

    return Created("creado de manera existosa",new 
    {
        newUser.UserId,
        newUser.UserName,
        newUser.Email,
    });
  }


  private string GenerarToken(User user){
    var claims=new List<Claim>
    {
      new Claim("userId",user.UserId.ToString()),
      new Claim(ClaimTypes.Name, user.NormalizedUserName),
      new Claim(ClaimTypes.Email, user.NormalizedEmail),
    };

    foreach (var userRole in user.UserRoles)
      {
        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
      };

    var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

    var credenciales= new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

    var generarToken=new JwtSecurityToken(
      claims:claims,
      expires:DateTime.Now.AddHours(8),
      signingCredentials:credenciales
    );

    return new JwtSecurityTokenHandler().WriteToken(generarToken);
  }
}