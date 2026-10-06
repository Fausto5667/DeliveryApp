using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

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
