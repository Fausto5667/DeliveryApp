using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidoServico _servico;

    public PedidosController(PedidoServico servico)
    {
        _servico = servico;
    }

    /// <summary>
    /// Menu 6: Criar e montar pedido com itens, cliente e entregador.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult CriarPedido([FromBody] CriarPedidoRequest request)
    {
        if (request is null)
            throw new DominioException("O corpo da requisição não pode ser vazio.");

        var itensTuplas = request.Itens.Select(i => (i.CodigoItem, i.Quantidade));
        var pedido = _servico.Criar(request.TelefoneCliente, request.EntregadorId, request.DistanciaKm, itensTuplas);
        var response = PedidoResponse.DeDominio(pedido);

        return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, response);
    }

    /// <summary>
    /// Menu 7: Avançar status sequencial de um pedido (Recebido -> EmPreparo -> Pronto -> EmRota -> Entregue).
    /// </summary>
    [HttpPatch("{id:int}/avancar")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AvancarStatus([FromRoute] int id)
    {
        var pedido = _servico.Avancar(id);
        return Ok(PedidoResponse.DeDominio(pedido));
    }

    /// <summary>
    /// Menu 8: Listar pedidos filtrados por status (ex: ?status=EmPreparo) ou listar todos se omitido.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoResponse>), StatusCodes.Status200OK)]
    public IActionResult ListarPedidos([FromQuery] StatusPedido? status)
    {
        var pedidos = _servico.Listar(status);
        var response = pedidos.Select(PedidoResponse.DeDominio).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Obter detalhes consolidados de um pedido por Id.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId([FromRoute] int id)
    {
        var pedido = _servico.ObterPorId(id);
        return Ok(PedidoResponse.DeDominio(pedido));
    }
}
