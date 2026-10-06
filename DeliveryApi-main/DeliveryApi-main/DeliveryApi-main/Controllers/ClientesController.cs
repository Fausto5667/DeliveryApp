using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ClienteServico _servico;

    public ClientesController(ClienteServico servico)
    {
        _servico = servico;
    }

    /// <summary>
    /// Menu 4: Cadastrar novo cliente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Cadastrar([FromBody] CriarClienteRequest request)
    {
        if (request is null)
            throw new DominioException("O corpo da requisição não pode ser vazio.");

        var cliente = _servico.Cadastrar(request.Nome, request.Telefone, request.Endereco);
        var response = ClienteResponse.DeDominio(cliente);
        return CreatedAtAction(nameof(ObterPorTelefone), new { telefone = cliente.Telefone }, response);
    }

    /// <summary>
    /// Listar todos os clientes cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClienteResponse>), StatusCodes.Status200OK)]
    public IActionResult Listar()
    {
        var clientes = _servico.Listar();
        var response = clientes.Select(ClienteResponse.DeDominio).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Obter cliente por telefone identificador único.
    /// </summary>
    [HttpGet("{telefone}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorTelefone([FromRoute] string telefone)
    {
        var cliente = _servico.ObterPorTelefone(telefone);
        return Ok(ClienteResponse.DeDominio(cliente));
    }
}
