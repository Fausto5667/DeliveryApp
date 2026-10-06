using System.Collections.Concurrent;
using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

/// <summary>
/// CONCEITO POO - POLIMORFISMO DE INTERFACE:
/// Implementação em memória do repositório de itens de cardápio.
/// Utiliza coleções seguras para concorrência em ambiente Web API.
/// </summary>
public class RepositorioItemCardapioMemoria : IRepositorio<ItemCardapio>
{
    private readonly ConcurrentDictionary<int, ItemCardapio> _itens = new();

    public void Adicionar(ItemCardapio entidade)
    {
        _itens[entidade.Codigo] = entidade;
    }

    public ItemCardapio? ObterPorId(int id)
    {
        _itens.TryGetValue(id, out var item);
        return item;
    }

    public IReadOnlyList<ItemCardapio> ListarTodos()
    {
        return _itens.Values.OrderBy(i => i.Codigo).ToList().AsReadOnly();
    }
}
