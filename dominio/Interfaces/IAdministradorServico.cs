using minimal_api.dominio.dto;
using minimal_api.dominio.entidades;

namespace minimal_api.dominio.Interfaces;

public interface IAdministradorServico
{
    Administrador? Login(LoginDTO loginDTO);
}

