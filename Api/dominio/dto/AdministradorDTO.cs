using minimal_api.dominio.Enuns;

namespace minimal_api.dominio.dto
{
    public class AdministradorDTO
    {
        public string Email { get; set; } = default!;

        public string Senha { get; set; } = default!;

        public Perfil? Perfil { get; set; } = default!;
    }
}
