using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// CONCEITO POO - COMPOSIÇÃO E ASSOCIAÇÃO:
/// Representa uma linha de item dentro do pedido com sua respectiva quantidade.
/// - Composição em relação ao Pedido: ItemPedido não tem ciclo de vida próprio fora do Pedido.
/// - Associação em relação ao ItemCardapio: associa-se polimorficamente a qualquer subtipo
///   (Prato, Bebida ou Sobremesa) sem acoplamento direto com as classes concretas.
/// </summary>
public class ItemPedido
{
    /// <summary>
    /// Item de cardápio associado (Prato, Bebida ou Sobremesa).
    /// </summary>
    public ItemCardapio Item { get; private set; }

    /// <summary>
    /// Quantidade de unidades deste item.
    /// </summary>
    public int Quantidade { get; private set; }

    /// <summary>
    /// Subtotal calculado pelo produto entre o preço base do item e a quantidade.
    /// </summary>
    public decimal Subtotal => Item.PrecoBase * Quantidade;

    public ItemPedido(ItemCardapio item, int quantidade)
    {
        Item = item ?? throw new DominioException("O item de cardápio é obrigatório.");

        if (quantidade <= 0)
            throw new DominioException("A quantidade do item deve ser maior que zero.");

        Quantidade = quantidade;
    }
}
