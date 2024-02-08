using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.dominio.dto;
using minimal_api.dominio.entidades;
using minimal_api.dominio.Enuns;
using minimal_api.dominio.Interfaces;
using minimal_api.dominio.ModelViews;
using minimal_api.dominio.servicos;
using minimal_api.infraestrutura.DB;

#region builder

var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();

if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer (option => {
    option.TokenValidationParameters = new TokenValidationParameters{
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();

builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<DbContexto>(options => {
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Pg")
    );
});

var app = builder.Build();

#endregion

#region Home

app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

#endregion

#region Administradores

string GerarTokenJwT(Administrador administrador){
    if(string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new("Email", administrador.Email),
        new("Perfil", administrador.Perfil),
        new(ClaimTypes.Role, administrador.Perfil),
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}


app.MapPost("/administradores/login", ([FromBody]LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    var adm = administradorServico.Login(loginDTO);

    if(adm != null){
       string token = GerarTokenJwT(adm);

       return Results.Ok(new AdministradorLogado
       {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
       });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");

app.MapPost("/administradores", ([FromBody]AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = new ErrosDeValidacao{
        Mensagens = []
    };

    if(string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("Email não pode ser vazio");
    if(string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("Senha não pode ser vazio");
    if(administradorDTO.Perfil == null )
        validacao.Mensagens.Add("Perfil não pode ser vazio");

    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var administrador = new Administrador{
        Email=  administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil?.ToString() ?? Perfil.Editor.ToString()
    };

    administradorServico.Incluir(administrador);

    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView{
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
  .WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery]int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);

    foreach( var adm in administradores)
    {
        adms.Add(new AdministradorModelView{
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }

    return Results.Ok(adms);

}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
  .WithTags("Administradores");

app.MapGet("/administrador/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscaPorId(id);

    if(administrador == null) return Results.NotFound();

    return Results.Ok(new AdministradorModelView{
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
  .WithTags("Administradores");

#endregion

#region  Veiculos

static ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao{
        Mensagens = []
    };

    if(string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ficar em branco");

    if(string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A Marca não pode ficar em branco");

    if(veiculoDTO.Ano < 1950)
        validacao.Mensagens.Add("Veiculo muito antigo, aceito somentes anos superiores a 1950");

    return validacao;
}

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {

    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm, Editor"})
  .WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {

    var veiculo = veiculoServico.BuscaPorId(id);

    if(veiculo == null) return Results.NotFound();

    return Results.Ok(veiculo);

}).RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm, Editor"})
  .WithTags("Veiculos");

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {

    var validacao = validaDTO(veiculoDTO);

    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var veiculo = new Veiculo{
        Nome=  veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
  .WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id,  VeiculoDTO veiculoDTO,  IVeiculoServico veiculoServico) => {

    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound();

    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);

}).RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
  .WithTags("Veiculos");;

app.MapDelete("/veiculos/{id}", ([FromRoute] int id,  IVeiculoServico veiculoServico) => {

    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(veiculo);

    return Results.NoContent();

}).RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
  .WithTags("Veiculos");;


#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

#endregion
