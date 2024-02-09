using minimal_api.dominio.dto;
using minimal_api.dominio.entidades;

namespace minimal_api.dominio.Interfaces;

public interface IAdministradorServico
{
    Administrador? Login(LoginDTO loginDTO);

    Administrador Incluir(Administrador administrador);

    Administrador? BuscaPorId(int Id);

    List<Administrador> Todos(int? pagina);
}

