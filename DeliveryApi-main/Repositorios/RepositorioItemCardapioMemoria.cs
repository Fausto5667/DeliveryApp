using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Repositorios;

public class RepositorioItemCardapioMemoria : RepositorioMemoria<ItemCardapio, int>
{
    protected override int ObterId(ItemCardapio item) => item.Codigo;

    protected override IEnumerable<ItemCardapio> Ordenar(IEnumerable<ItemCardapio> itens) =>
        itens.OrderBy(i => i.Codigo);
}
