using minimal_api.Infrastructure.Db;
using minimal_api.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using minimal_api.Domain.ModelViews;
using minimal_api.Migrations;
using minimal_api.Domain.Entities;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http.HttpResults;
using minimal_api.Domain.Enuns;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.VisualBasic;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if(!string.IsNullOrEmpty(key))
    key = "minimal-api-chave-secreta-ajustada32";

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>{
    option.TokenValidationParameters = new TokenValidationParameters{
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdmServices, AdmServices>();
builder.Services.AddScoped<IVehicleServices, VehicleServices>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Jwt",
        In = ParameterLocation.Header,
        Description = "Insira o Token Jwt aqui"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {   
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<DatabaseContext>(options =>{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores
string GerarTokenJwt(Adm adm){
    if(string.IsNullOrEmpty(key))
        return string.Empty;
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", adm.Email),
        new Claim("Perfil", adm.Perfil),
        new Claim(ClaimTypes.Role, adm.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdmServices admServices) => {
    var adm = admServices.Login(loginDTO);
    if(adm != null){
        string token = GerarTokenJwt(adm);

        return Results.Ok(new AdmLogged
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }
    
}).AllowAnonymous().WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdmDTO admDTO, IAdmServices admServices) => {
    var validacao = new ValidationErrors{
        Mensagens = new List<string>()
    };

    if(string.IsNullOrEmpty(admDTO.Email))
        validacao.Mensagens.Add("Email não pode ser vazio");
    
    if(string.IsNullOrEmpty(admDTO.Senha))
        validacao.Mensagens.Add("Senha não pode ser vazia");
    
    if(admDTO.Perfil == null)
        validacao.Mensagens.Add("Perfil não pode ser vazio");

    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var administrador = new Adm{
        Email = admDTO.Email,
        Senha = admDTO.Senha,
        Perfil = admDTO.Perfil.ToString() ?? Profile.Editor.ToString()
    };
    admServices.Incluir(administrador);

    return Results.Created($"/administrador/{administrador.Id}", Results.Ok(new AdmModelView{
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    }));
}).RequireAuthorization().RequireAuthorization(
    new AuthorizeAttribute{Roles = "Adm"}
).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int pagina, IAdmServices admServices) => {   
    var adms = new List<AdmModelView>();
    var administradores = admServices.Todos(pagina);
    foreach(var adm in administradores)
    {
        adms.Add(new AdmModelView{
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(administradores);
}).RequireAuthorization().RequireAuthorization(
    new AuthorizeAttribute{Roles = "Adm"}
).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdmServices admServices) => {   
    var administrador = admServices.BuscaPorId(id);
    if(administrador == null)
        return Results.NotFound();
    return Results.Ok(new AdmModelView{
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).RequireAuthorization().RequireAuthorization(
    new AuthorizeAttribute{Roles = "Adm"}
).WithTags("Administradores");

app.MapDelete("/administradores/{id}", ([FromRoute] int id, IAdmServices admServices) => {
    
    var administrador = admServices.BuscaPorId(id);
    if(administrador == null)
        return Results.NotFound();

    admServices.Apagar(administrador);
    return Results.NoContent();
}).RequireAuthorization().WithTags("Administradores");
#endregion

#region Veiculos
ValidationErrors validationDTO(VehicleDTO vehicleDTO)
{
    var validacao = new ValidationErrors{
        Mensagens = new List<string>()
    };

    if(string.IsNullOrEmpty(vehicleDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ser vazio.");
    
    if(string.IsNullOrEmpty(vehicleDTO.Marca))
        validacao.Mensagens.Add("O marca não pode ficar em branco.");

    if(vehicleDTO.Ano < 1950)
        validacao.Mensagens.Add("Veículo muito antigo, aceito apenas anos superiores a 1950.");
    
    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VehicleDTO vehicleDTO, IVehicleServices vehicleServices) => {   
    var validacao = validationDTO(vehicleDTO);
    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var veiculo = new Vehicle{
        Nome = vehicleDTO.Nome,
        Marca = vehicleDTO.Marca,
        Ano = vehicleDTO.Ano
    };
    vehicleServices.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization().RequireAuthorization(
    new AuthorizeAttribute{Roles = "Adm, Editor"}
).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int pagina, IVehicleServices vehicleServices) => {   
    var veiculos = vehicleServices.Todos(pagina);
    return Results.Ok(veiculos);
}).RequireAuthorization().RequireAuthorization(
    new AuthorizeAttribute{Roles = "Adm, Editor"}
).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVehicleServices vehicleServices) => {   
    var veiculo = vehicleServices.BuscaPorId(id);
    if(veiculo == null)
        return Results.NotFound();
    return Results.Ok(veiculo);
}).RequireAuthorization().RequireAuthorization(
    new AuthorizeAttribute{Roles = "Adm, Editor"}
).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleServices vehicleServices) => {  
    var veiculo = vehicleServices.BuscaPorId(id);
    if(veiculo == null)
        return Results.NotFound();

    var validacao = validationDTO(vehicleDTO);
    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    veiculo.Nome = vehicleDTO.Nome;
    veiculo.Marca = vehicleDTO.Marca;
    veiculo.Ano = vehicleDTO.Ano;

    vehicleServices.Atualizar(veiculo);
    return Results.Ok(veiculo);

}).RequireAuthorization().RequireAuthorization(
    new AuthorizeAttribute{Roles = "Adm"}
).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVehicleServices vehicleServices) => {
    
    var veiculo = vehicleServices.BuscaPorId(id);
    if(veiculo == null)
        return Results.NotFound();

    vehicleServices.Apagar(veiculo);
    return Results.NoContent();
}).RequireAuthorization().RequireAuthorization(
    new AuthorizeAttribute{Roles = "Adm"}
).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
