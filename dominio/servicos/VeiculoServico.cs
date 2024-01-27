using Microsoft.EntityFrameworkCore;
using minimal_api.dominio.dto;
using minimal_api.dominio.entidades;
using minimal_api.dominio.Interfaces;
using minimal_api.infraestrutura.DB;

namespace minimal_api.dominio.servicos;


public class VeiculoServico : IVeiculoServico
{
    private readonly DbContexto _contexto;

    public void Apagar(Veiculo veiculo)
    {
        _contexto.Veiculos.Remove(veiculo);
        _contexto.SaveChanges();
    }

    public void Atualizar(Veiculo veiculo)
    {
        _contexto.Veiculos.Update(veiculo);
        _contexto.SaveChanges();
    }

    public Veiculo? BuscaPorId(int id)
    {
        return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
    }

    public void Incluir(Veiculo veiculo)
    {
        _contexto.Veiculos.Add(veiculo);
        _contexto.SaveChanges();
    }

    public List<Veiculo> Todos(int pagina = 1, string? nome = null, string? marca = null)
    {
        var query = _contexto.Veiculos.AsQueryable();

        if(!string.IsNullOrEmpty(nome))
        {
            query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
        }

        int itensPorPagina = 10;

        query = query.Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina);

        return [.. query];
    }
}

