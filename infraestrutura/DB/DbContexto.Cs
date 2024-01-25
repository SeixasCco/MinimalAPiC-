using Microsoft.EntityFrameworkCore;
using minimal_api.dominio.entidades;

namespace minimal_api.infraestrutura.DB;


public class DbContexto : DbContext
{
    private readonly IConfiguration _configuracaoAppSettings;
    public DbContexto(IConfiguration configuracaoAppSettings)
    {
        _configuracaoAppSettings = configuracaoAppSettings;
    }
    public DbSet<Administrador> Administradores {get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configuracaoAppSettings.GetConnectionString("pg")?.ToString();
            if(!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseNpgsql(stringConexao);
            }else
            {
            throw new InvalidOperationException("The database connection string 'pg' was not found.");
            }
        }

    }
}
