using Microsoft.EntityFrameworkCore;
using GymApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddDbContext<GimnasioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var key= builder.Configuration["Jwt:Key"];

    options.TokenValidationParameters=new TokenValidationParameters
    {
        ValidateIssuer=false,
        ValidateAudience=false,
        ValidateLifetime=true,
        ValidateIssuerSigningKey=true,
        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
    policy=>policy.WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader());
});

//para probar con swagger los endpoint 
// builder.Services.AddSwaggerGen(options =>
// {
//     options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
//         Scheme = "bearer",
//         BearerFormat = "JWT",
//         In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//         Description = "Pon aquí: Bearer {tu_token}"
//     });

//     options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//     {
//         {
//             new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//             {
//                 Reference = new Microsoft.OpenApi.Models.OpenApiReference
//                 {
//                     Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 }
//             },
//             new string[] {}
//         }
//     });
// });

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
// app.UseAuthorization();
app.MapControllers();


app.Run();

