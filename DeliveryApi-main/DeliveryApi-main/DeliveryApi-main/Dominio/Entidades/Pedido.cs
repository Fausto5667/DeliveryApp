using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// CONCEITO POO - AGREGAÇÃO, COMPOSIÇÃO, ASSOCIAÇÃO E ENCAPSULAMENTO:
/// - Associação: Pedido associa-se a Cliente e a Entregador (objetos com ciclo de vida independente).
/// - Composição: A lista interna de itens (_itens) pertence exclusivamente a este pedido.
/// - Polimorfismo: O cálculo do tempo de preparo delega para cada ItemCardapio sem checagem de tipo.
/// - Encapsulamento: Estados e coleções são imutáveis externamente; transições ocorrem via métodos de domínio.
/// </summary>
public class Pedido
{
    private readonly List<ItemPedido> _itens = new();

    public int Id { get; private set; }

    /// <summary>
    /// CONCEITO POO - ASSOCIAÇÃO:
    /// Referência ao cliente que solicitou o pedido.
    /// </summary>
    public Cliente Cliente { get; private set; }

    /// <summary>
    /// CONCEITO POO - ASSOCIAÇÃO:
    /// Referência ao entregador responsável pela rota logística.
    /// </summary>
    public Entregador Entregador { get; private set; }

    /// <summary>
    /// Distância estimada em quilômetros até o endereço de entrega.
    /// </summary>
    public decimal DistanciaKm { get; private set; }

    /// <summary>
    /// CONCEITO POO - COMPOSIÇÃO E ENCAPSULAMENTO:
    /// Coleção interna exposta como IReadOnlyList, impedindo alterações externas diretas.
    /// </summary>
    public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();

    /// <summary>
    /// Estado atual do pedido dentro da máquina de estados.
    /// </summary>
    public StatusPedido Status { get; private set; }

    /// <summary>
    /// Data e hora de criação do pedido.
    /// </summary>
    public DateTime CriadoEm { get; private set; }

    /// <summary>
    /// Data e hora da conclusão da entrega.
    /// </summary>
    public DateTime? EntregueEm { get; private set; }

    public Pedido(int id, Cliente cliente, Entregador entregador, decimal distanciaKm)
    {
        if (id <= 0)
            throw new DominioException("O identificador do pedido deve ser maior que zero.");

        Cliente = cliente ?? throw new DominioException("O cliente do pedido é obrigatório.");
        Entregador = entregador ?? throw new DominioException("O entregador do pedido é obrigatório.");

        if (distanciaKm <= 0)
            throw new DominioException("A distância em quilômetros deve ser maior que zero.");

        Id = id;
        DistanciaKm = distanciaKm;
        Status = StatusPedido.Recebido;
        CriadoEm = DateTime.Now;
    }

    /// <summary>
    /// Adiciona um item ao pedido enquanto ele estiver no estado inicial 'Recebido'.
    /// </summary>
    public void AdicionarItem(ItemCardapio item, int quantidade)
    {
        if (Status != StatusPedido.Recebido)
            throw new DominioException("Não é permitido adicionar itens a um pedido em andamento.");

        _itens.Add(new ItemPedido(item, quantidade));
    }

    /// <summary>
    /// Soma dos subtotais de todas as linhas de itens do pedido.
    /// </summary>
    public decimal Subtotal => _itens.Sum(i => i.Subtotal);

    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// A confecção dos pratos ocorre de forma paralela na cozinha; logo, o tempo total é determinado
    /// pelo item de maior duração de preparo. Graças ao polimorfismo, chamamos i.Item.TempoPreparoMinutos
    /// de maneira uniforme: Prato retorna seu tempo real e Bebida/Sobremesa retornam 0 sem if de tipo!
    /// </summary>
    public int TempoPreparoMinutos => _itens.Any() ? _itens.Max(i => i.Item.TempoPreparoMinutos) : 0;

    /// <summary>
    /// Regra de negócio: frete calculado pela taxa por km do veículo multiplicada pela distância.
    /// Se o subtotal atingir ou superar R$ 80,00, o frete é gratuito (R$ 0,00).
    /// </summary>
    public decimal Frete => Subtotal >= RegrasDeNegocio.MetaFreteGratis
        ? 0m
        : RegrasDeNegocio.TaxaPorKm(Entregador.Veiculo) * DistanciaKm;

    /// <summary>
    /// Valor final do pedido acumulando itens e frete.
    /// </summary>
    public decimal Total => Subtotal + Frete;

    /// <summary>
    /// Finaliza a montagem inicial do pedido e aloca o entregador selecionado.
    /// </summary>
    public void Confirmar()
    {
        if (!_itens.Any())
            throw new DominioException("Pedido sem itens.");

        Entregador.Ocupar();
    }

    /// <summary>
    /// MÁQUINA DE ESTADOS SEQUENCIAL:
    /// Transiciona o pedido estritamente pela sequência de negócio:
    /// Recebido -> EmPreparo -> Pronto -> EmRota -> Entregue.
    /// Ao finalizar a entrega, registra o horário e libera o entregador para novas rotas.
    /// </summary>
    public void Avancar()
    {
        switch (Status)
        {
            case StatusPedido.Recebido:
                Status = StatusPedido.EmPreparo;
                break;

            case StatusPedido.EmPreparo:
                Status = StatusPedido.Pronto;
                break;

            case StatusPedido.Pronto:
                Status = StatusPedido.EmRota;
                break;

            case StatusPedido.EmRota:
                Status = StatusPedido.Entregue;
                EntregueEm = DateTime.Now;
                Entregador.Liberar();
                break;

            case StatusPedido.Entregue:
                throw new DominioException("Pedido já foi entregue.");

            default:
                throw new DominioException("Status desconhecido do pedido.");
        }
    }
}
