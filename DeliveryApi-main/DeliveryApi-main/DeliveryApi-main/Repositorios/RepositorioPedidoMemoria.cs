using System.Collections.Concurrent;
using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

/// <summary>
/// CONCEITO POO - POLIMORFISMO DE INTERFACE:
/// Implementação em memória do repositório de pedidos.
/// </summary>
public class RepositorioPedidoMemoria : IRepositorio<Pedido>
{
    private readonly ConcurrentDictionary<int, Pedido> _pedidos = new();

    public void Adicionar(Pedido entidade)
    {
        _pedidos[entidade.Id] = entidade;
    }

    public Pedido? ObterPorId(int id)
    {
        _pedidos.TryGetValue(id, out var pedido);
        return pedido;
    }

    public IReadOnlyList<Pedido> ListarTodos()
    {
        return _pedidos.Values.OrderBy(p => p.Id).ToList().AsReadOnly();
    }
}
