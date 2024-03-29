using Microsoft.EntityFrameworkCore;
using minimal_api.dominio.entidades;

namespace minimal_api.infraestrutura.DB;


public class DbContexto(IConfiguration configuracaoAppSettings) : DbContext
{
    private readonly IConfiguration _configuracaoAppSettings = configuracaoAppSettings;

    public DbSet<Veiculo> Veiculos {get; set;} = default!;
    public DbSet<Administrador> Administradores {get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador {
                Id = 1,
                Email = "Administrador@teste.com",
                Senha = "123456",
                Perfil = "Adm"
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configuracaoAppSettings.GetConnectionString("Pg")?.ToString();
            if(!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseNpgsql(stringConexao);
            }else
            {
            throw new InvalidOperationException("The database connection string 'Pg' was not found.");
            }
        }

    }
}
