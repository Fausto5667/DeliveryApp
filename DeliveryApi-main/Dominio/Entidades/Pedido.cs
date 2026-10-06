using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dominio.Frete;

namespace DeliveryApi.Dominio.Entidades;

/// <summary>
/// Pedido: agrega Cliente e Entregador (existem antes e depois do pedido e são compartilhados com outros
/// pedidos) e é composto por suas linhas ItemPedido (nascem e morrem com ele).
/// Todo o ciclo de vida passa por métodos de domínio: AdicionarItem, Confirmar e Avancar.
/// </summary>
public class Pedido
{
    private readonly List<ItemPedido> _itens = new();
    private readonly object _trava = new();

    public int Id { get; private set; }
    public Cliente Cliente { get; private set; }
    public Entregador Entregador { get; private set; }
    public decimal DistanciaKm { get; private set; }
    public StatusPedido Status { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? EntregueEm { get; private set; }

    /// <summary>Frete definido na confirmação; fica fixo mesmo que as tarifas mudem depois.</summary>
    public decimal Frete { get; private set; }

    public bool Confirmado { get; private set; }

    public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();

    public decimal Subtotal => _itens.Sum(i => i.Subtotal);

    public decimal Total => Subtotal + Frete;

    /// <summary>
    /// O preparo é paralelo na cozinha, então o tempo do pedido é o do item mais demorado (não a soma).
    /// Bebidas e sobremesas retornam 0, por isso não há necessidade de filtrar por tipo.
    /// </summary>
    public int TempoPreparoMinutos => _itens.Count > 0 ? _itens.Max(i => i.Item.TempoPreparoMinutos) : 0;

    public StatusPedido? ProximoStatus => Status.ObterProximo();

    public Pedido(int id, Cliente cliente, Entregador entregador, decimal distanciaKm)
    {
        if (id <= 0)
            throw new DominioException("O identificador do pedido deve ser maior que zero.");

        Cliente = cliente ?? throw new DominioException("O cliente do pedido é obrigatório.");
        Entregador = entregador ?? throw new DominioException("O entregador do pedido é obrigatório.");

        if (distanciaKm <= 0)
            throw new DominioException("A distância em quilômetros deve ser maior que zero.");

        if (distanciaKm > RegrasDeNegocio.DistanciaMaximaKm)
            throw new DominioException($"A distância não pode ultrapassar {RegrasDeNegocio.DistanciaMaximaKm:N0} km.");

        Id = id;
        DistanciaKm = distanciaKm;
        Status = StatusPedido.Recebido;
        CriadoEm = DateTime.Now;
    }

    /// <summary>Adiciona um item; se ele já está no pedido, apenas soma a quantidade.</summary>
    public void AdicionarItem(ItemCardapio item, int quantidade)
    {
        if (item is null)
            throw new DominioException("O item de cardápio é obrigatório.");

        if (Confirmado)
            throw new DominioException("Não é possível alterar os itens de um pedido já confirmado.");

        var existente = _itens.FirstOrDefault(i => i.Item.Codigo == item.Codigo);
        if (existente is null)
            _itens.Add(new ItemPedido(item, quantidade));
        else
            existente.Acrescentar(quantidade);
    }

    /// <summary>Frete que este pedido teria com a calculadora informada, sem alterar o pedido.</summary>
    public decimal CalcularFrete(ICalculadoraFrete calculadora) =>
        calculadora.Calcular(Entregador.Veiculo, DistanciaKm, Subtotal);

    /// <summary>
    /// Fecha a montagem do pedido: exige ao menos um item, fixa o frete e aloca o entregador.
    /// O frete é calculado antes de ocupar o entregador para que uma falha não deixe o entregador preso.
    /// </summary>
    public void Confirmar(ICalculadoraFrete calculadora)
    {
        if (Confirmado)
            throw new DominioException("O pedido já foi confirmado.");

        if (_itens.Count == 0)
            throw new DominioException("Pedido sem itens.");

        var frete = CalcularFrete(calculadora);

        Entregador.Ocupar();
        Frete = frete;
        Confirmado = true;
    }

    /// <summary>
    /// Avança um passo na sequência Recebido, EmPreparo, Pronto, EmRota, Entregue, sem permitir pular etapas.
    /// Ao entregar, registra o horário e libera o entregador.
    /// </summary>
    public void Avancar()
    {
        lock (_trava)
        {
            if (!Confirmado)
                throw new DominioException("O pedido ainda não foi confirmado.");

            var proximo = Status.ObterProximo()
                ?? throw new ConflitoException("Pedido já foi entregue.");

            Status = proximo;

            if (proximo == StatusPedido.Entregue)
            {
                EntregueEm = DateTime.Now;
                Entregador.Liberar();
            }
        }
    }
}
