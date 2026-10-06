using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

/// <summary>
/// Camada de Serviço que orquestra a criação, montagem e avanço do ciclo de vida dos pedidos.
/// Faz a ponte entre clientes, cardápio, entregadores e o repositório de pedidos.
/// </summary>
public class PedidoServico
{
    private readonly IRepositorio<Pedido> _repositorioPedido;
    private readonly IClienteRepositorio _repositorioCliente;
    private readonly IRepositorio<Entregador> _repositorioEntregador;
    private readonly IRepositorio<ItemCardapio> _repositorioCardapio;
    private int _contadorId = 0;

    public PedidoServico(
        IRepositorio<Pedido> repositorioPedido,
        IClienteRepositorio repositorioCliente,
        IRepositorio<Entregador> repositorioEntregador,
        IRepositorio<ItemCardapio> repositorioCardapio)
    {
        _repositorioPedido = repositorioPedido;
        _repositorioCliente = repositorioCliente;
        _repositorioEntregador = repositorioEntregador;
        _repositorioCardapio = repositorioCardapio;
    }

    private int ProximoId() => Interlocked.Increment(ref _contadorId);

    public Pedido Criar(string telefoneCliente, int entregadorId, decimal distanciaKm, IEnumerable<(int CodigoItem, int Quantidade)> itensSolicitados)
    {
        var cliente = _repositorioCliente.ObterPorTelefone(telefoneCliente)
            ?? throw new NaoEncontradoException($"Cliente com telefone '{telefoneCliente}' não encontrado.");

        var entregador = _repositorioEntregador.ObterPorId(entregadorId)
            ?? throw new NaoEncontradoException($"Entregador com identificador {entregadorId} não encontrado.");

        if (itensSolicitados is null || !itensSolicitados.Any())
            throw new DominioException("Pedido sem itens.");

        var pedido = new Pedido(ProximoId(), cliente, entregador, distanciaKm);

        foreach (var (codigoItem, quantidade) in itensSolicitados)
        {
            var itemCardapio = _repositorioCardapio.ObterPorId(codigoItem)
                ?? throw new NaoEncontradoException($"Item de cardápio com código {codigoItem} não encontrado.");

            pedido.AdicionarItem(itemCardapio, quantidade);
        }

        pedido.Confirmar();
        _repositorioPedido.Adicionar(pedido);
        return pedido;
    }

    public Pedido Avancar(int id)
    {
        var pedido = ObterPorId(id);
        pedido.Avancar();
        return pedido;
    }

    public IReadOnlyList<Pedido> Listar(StatusPedido? status = null)
    {
        var todos = _repositorioPedido.ListarTodos();
        if (status.HasValue)
        {
            return todos.Where(p => p.Status == status.Value).ToList().AsReadOnly();
        }

        return todos;
    }

    public Pedido ObterPorId(int id)
    {
        var pedido = _repositorioPedido.ObterPorId(id);
        if (pedido is null)
            throw new NaoEncontradoException($"Pedido com identificador {id} não encontrado.");

        return pedido;
    }
}
