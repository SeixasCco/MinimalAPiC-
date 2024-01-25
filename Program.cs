using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using minimal_api.dominio.dto;
using minimal_api.infraestrutura.DB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("pg")
    );
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) => {
    if(loginDTO.Email == "adm@teste.com" && loginDTO.Senha == "123456"){
        return Results.Ok("Login com sucesso");
    }else
        return Results.Unauthorized();
});


app.Run();

