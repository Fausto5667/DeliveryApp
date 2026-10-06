using System.Collections.Concurrent;
using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

/// <summary>
/// CONCEITO POO - POLIMORFISMO DE INTERFACE:
/// Implementação em memória do repositório de entregadores.
/// </summary>
public class RepositorioEntregadorMemoria : IRepositorio<Entregador>
{
    private readonly ConcurrentDictionary<int, Entregador> _entregadores = new();

    public void Adicionar(Entregador entidade)
    {
        _entregadores[entidade.Id] = entidade;
    }

    public Entregador? ObterPorId(int id)
    {
        _entregadores.TryGetValue(id, out var entregador);
        return entregador;
    }

    public IReadOnlyList<Entregador> ListarTodos()
    {
        return _entregadores.Values.OrderBy(e => e.Id).ToList().AsReadOnly();
    }
}
