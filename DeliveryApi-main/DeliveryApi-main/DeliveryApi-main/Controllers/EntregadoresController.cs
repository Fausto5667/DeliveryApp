using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntregadoresController : ControllerBase
{
    private readonly EntregadorServico _servico;

    public EntregadoresController(EntregadorServico servico)
    {
        _servico = servico;
    }

    /// <summary>
    /// Menu 5: Cadastrar novo entregador na frota.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EntregadorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Cadastrar([FromBody] CriarEntregadorRequest request)
    {
        if (request is null)
            throw new DominioException("O corpo da requisição não pode ser vazio.");

        var entregador = _servico.Cadastrar(request.Nome, request.Veiculo);
        var response = EntregadorResponse.DeDominio(entregador);
        return CreatedAtAction(nameof(ObterPorId), new { id = entregador.Id }, response);
    }

    /// <summary>
    /// Listar entregadores da frota com seu respectivo indicador de disponibilidade.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EntregadorResponse>), StatusCodes.Status200OK)]
    public IActionResult Listar()
    {
        var entregadores = _servico.Listar();
        var response = entregadores.Select(EntregadorResponse.DeDominio).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Obter entregador por identificador único.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EntregadorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId([FromRoute] int id)
    {
        var entregador = _servico.ObterPorId(id);
        return Ok(EntregadorResponse.DeDominio(entregador));
    }
}
