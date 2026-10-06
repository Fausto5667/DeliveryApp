using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dominio.Frete;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

/// <summary>Orquestra a criação, a simulação e o avanço de status dos pedidos.</summary>
public class PedidoServico
{
    private const int IdDaSimulacao = 1;

    private readonly IRepositorio<Pedido, int> _repositorioPedido;
    private readonly IRepositorio<Cliente, string> _repositorioCliente;
    private readonly IRepositorio<Entregador, int> _repositorioEntregador;
    private readonly IRepositorio<ItemCardapio, int> _repositorioCardapio;
    private readonly ICalculadoraFrete _calculadoraFrete;
    private readonly object _trava = new();
    private int _proximoId = 1;

    public PedidoServico(
        IRepositorio<Pedido, int> repositorioPedido,
        IRepositorio<Cliente, string> repositorioCliente,
        IRepositorio<Entregador, int> repositorioEntregador,
        IRepositorio<ItemCardapio, int> repositorioCardapio,
        ICalculadoraFrete calculadoraFrete)
    {
        _repositorioPedido = repositorioPedido;
        _repositorioCliente = repositorioCliente;
        _repositorioEntregador = repositorioEntregador;
        _repositorioCardapio = repositorioCardapio;
        _calculadoraFrete = calculadoraFrete;
    }

    public Pedido Criar(NovoPedido dados)
    {
        lock (_trava)
        {
            var pedido = Montar(_proximoId, dados);
            pedido.Confirmar(_calculadoraFrete);
            _repositorioPedido.Adicionar(pedido);
            _proximoId++;
            return pedido;
        }
    }

    /// <summary>
    /// Calcula subtotal, frete e total sem gravar o pedido e sem ocupar o entregador.
    /// Usa o mesmo caminho de montagem do pedido real, então as regras nunca divergem.
    /// </summary>
    public SimulacaoPedido Simular(NovoPedido dados)
    {
        var pedido = Montar(IdDaSimulacao, dados);
        var frete = pedido.CalcularFrete(_calculadoraFrete);

        return new SimulacaoPedido(pedido.Subtotal, frete, pedido.Subtotal + frete, pedido.TempoPreparoMinutos);
    }

    public Pedido Avancar(int id)
    {
        var pedido = ObterPorId(id);
        pedido.Avancar();
        return pedido;
    }

    public IReadOnlyList<Pedido> Listar(StatusPedido? status = null)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
            throw new DominioException("Status inválido. Use Recebido, EmPreparo, Pronto, EmRota ou Entregue.");

        var todos = _repositorioPedido.ListarTodos();

        return status.HasValue
            ? todos.Where(p => p.Status == status.Value).ToList().AsReadOnly()
            : todos;
    }

    public Pedido ObterPorId(int id) =>
        _repositorioPedido.ObterPorId(id)
            ?? throw new NaoEncontradoException($"Pedido com identificador {id} não encontrado.");

    private Pedido Montar(int id, NovoPedido dados)
    {
        if (dados.Itens is null || dados.Itens.Count == 0)
            throw new DominioException("Pedido sem itens.");

        var telefone = Cliente.NormalizarTelefone(dados.TelefoneCliente);

        var cliente = _repositorioCliente.ObterPorId(telefone)
            ?? throw new NaoEncontradoException($"Cliente com telefone '{dados.TelefoneCliente}' não encontrado.");

        var entregador = _repositorioEntregador.ObterPorId(dados.EntregadorId)
            ?? throw new NaoEncontradoException($"Entregador com identificador {dados.EntregadorId} não encontrado.");

        var pedido = new Pedido(id, cliente, entregador, dados.DistanciaKm);

        foreach (var solicitado in dados.Itens)
        {
            if (solicitado is null)
                throw new DominioException("A lista de itens não pode conter elementos vazios.");

            var item = _repositorioCardapio.ObterPorId(solicitado.CodigoItem)
                ?? throw new NaoEncontradoException($"Item de cardápio com código {solicitado.CodigoItem} não encontrado.");

            pedido.AdicionarItem(item, solicitado.Quantidade);
        }

        return pedido;
    }
}
