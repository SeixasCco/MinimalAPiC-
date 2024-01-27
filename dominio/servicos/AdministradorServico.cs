using Microsoft.EntityFrameworkCore;
using minimal_api.dominio.dto;
using minimal_api.dominio.entidades;
using minimal_api.dominio.Interfaces;
using minimal_api.infraestrutura.DB;

namespace minimal_api.dominio.servicos;


public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;
    public AdministradorServico(DbContexto contexto)
    {
        _contexto = contexto;
    }
    public Administrador? Login(LoginDTO loginDTO)
    {
        var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        return adm;
    }
}

