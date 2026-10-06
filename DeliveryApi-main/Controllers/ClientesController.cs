using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly ClienteServico _servico;

    public ClientesController(ClienteServico servico)
    {
        _servico = servico;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Cadastrar([FromBody] CriarClienteRequest request)
    {
        var cliente = _servico.Cadastrar(request.Nome, request.Telefone, request.Endereco);
        return CreatedAtAction(nameof(ObterPorTelefone), new { telefone = cliente.Telefone }, ClienteResponse.DeDominio(cliente));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClienteResponse>), StatusCodes.Status200OK)]
    public IActionResult Listar() =>
        Ok(_servico.Listar().Select(ClienteResponse.DeDominio).ToList());

    [HttpGet("{telefone}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorTelefone(string telefone) =>
        Ok(ClienteResponse.DeDominio(_servico.ObterPorTelefone(telefone)));
}
