using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/cardapio")]
public class CardapioController : ControllerBase
{
    private readonly CardapioServico _servico;

    public CardapioController(CardapioServico servico)
    {
        _servico = servico;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ItemCardapioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AdicionarItem([FromBody] CriarItemRequest request)
    {
        var item = _servico.Adicionar(request.ParaComando());
        return CreatedAtAction(nameof(ObterPorCodigo), new { codigo = item.Codigo }, ItemCardapioResponse.DeDominio(item));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ItemCardapioResponse>), StatusCodes.Status200OK)]
    public IActionResult ListarCardapio() =>
        Ok(_servico.Listar().Select(ItemCardapioResponse.DeDominio).ToList());

    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IReadOnlyList<ItemCardapioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult BuscarPorNome([FromQuery] string? nome) =>
        Ok(_servico.BuscarPorNome(nome).Select(ItemCardapioResponse.DeDominio).ToList());

    [HttpGet("{codigo:int}")]
    [ProducesResponseType(typeof(ItemCardapioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorCodigo(int codigo) =>
        Ok(ItemCardapioResponse.DeDominio(_servico.ObterPorCodigo(codigo)));
}
