using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// Linha de um pedido: um item do cardápio e sua quantidade. É parte do Pedido (composição): só é criada
/// por Pedido.AdicionarItem e não existe fora dele. Já o ItemCardapio referenciado continua existindo
/// independentemente do pedido (associação).
/// </summary>
public class ItemPedido
{
    public ItemCardapio Item { get; private set; }
    public int Quantidade { get; private set; }

    public decimal Subtotal => Item.PrecoBase * Quantidade;

    internal ItemPedido(ItemCardapio item, int quantidade)
    {
        Item = item ?? throw new DominioException("O item de cardápio é obrigatório.");
        ValidarQuantidade(quantidade);
        Quantidade = quantidade;
    }

    internal void Acrescentar(int quantidade)
    {
        ValidarQuantidade(quantidade);
        ValidarQuantidade(Quantidade + quantidade);
        Quantidade += quantidade;
    }

    private static void ValidarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new DominioException("A quantidade do item deve ser maior que zero.");

        if (quantidade > RegrasDeNegocio.QuantidadeMaximaPorItem)
            throw new DominioException($"A quantidade de um item não pode ultrapassar {RegrasDeNegocio.QuantidadeMaximaPorItem} unidades.");
    }
}
