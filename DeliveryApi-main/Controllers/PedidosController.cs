using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly PedidoServico _servico;

    public PedidosController(PedidoServico servico)
    {
        _servico = servico;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult CriarPedido([FromBody] CriarPedidoRequest request)
    {
        var pedido = _servico.Criar(request.ParaComando());
        return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, PedidoResponse.DeDominio(pedido));
    }

    [HttpPost("simulacao")]
    [ProducesResponseType(typeof(SimulacaoPedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Simular([FromBody] CriarPedidoRequest request) =>
        Ok(SimulacaoPedidoResponse.DeDominio(_servico.Simular(request.ParaComando())));

    [HttpPatch("{id:int}/avancar")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult AvancarStatus(int id) =>
        Ok(PedidoResponse.DeDominio(_servico.Avancar(id)));

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ListarPedidos([FromQuery] StatusPedido? status) =>
        Ok(_servico.Listar(status).Select(PedidoResponse.DeDominio).ToList());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId(int id) =>
        Ok(PedidoResponse.DeDominio(_servico.ObterPorId(id)));
}
