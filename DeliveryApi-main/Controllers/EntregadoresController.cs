using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/entregadores")]
public class EntregadoresController : ControllerBase
{
    private readonly EntregadorServico _servico;

    public EntregadoresController(EntregadorServico servico)
    {
        _servico = servico;
    }

    /// <summary>Menu 5: cadastra um entregador (nasce disponível).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EntregadorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Cadastrar([FromBody] CriarEntregadorRequest request)
    {
        var entregador = _servico.Cadastrar(request.Nome, request.Veiculo!.Value);
        return CreatedAtAction(nameof(ObterPorId), new { id = entregador.Id }, EntregadorResponse.DeDominio(entregador));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EntregadorResponse>), StatusCodes.Status200OK)]
    public IActionResult Listar() =>
        Ok(_servico.Listar().Select(EntregadorResponse.DeDominio).ToList());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EntregadorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId(int id) =>
        Ok(EntregadorResponse.DeDominio(_servico.ObterPorId(id)));
}
