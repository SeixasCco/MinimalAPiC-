using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.dominio.dto;
using minimal_api.dominio.Interfaces;
using minimal_api.dominio.servicos;
using minimal_api.infraestrutura.DB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("pg")
    );
});

var app = builder.Build();


app.MapGet("/", () => "Hello World!");
app.MapPost("/login", ([FromBody]LoginDTO loginDTO, IAdministradorServico administradorServico) => {
    if(administradorServico.Login(loginDTO)!= null)
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
});


app.Run();

