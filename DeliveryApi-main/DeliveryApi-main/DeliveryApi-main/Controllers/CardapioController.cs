using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Excecoes;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardapioController : ControllerBase
{
    private readonly CardapioServico _servico;

    public CardapioController(CardapioServico servico)
    {
        _servico = servico;
    }

    /// <summary>
    /// Menu 1: Adicionar item ao cardápio (prato, bebida ou sobremesa).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ItemCardapioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AdicionarItem([FromBody] CriarItemRequest request)
    {
        if (request is null)
            throw new DominioException("O corpo da requisição não pode ser vazio.");

        ItemCardapio novoItem = request.Tipo.Trim().ToLowerInvariant() switch
        {
            "prato" => _servico.AdicionarPrato(
                request.Nome,
                request.PrecoBase,
                request.TempoPreparo ?? throw new DominioException("Tempo de preparo é obrigatório para pratos.")
            ),
            "bebida" => _servico.AdicionarBebida(
                request.Nome,
                request.PrecoBase,
                request.VolumeMl ?? throw new DominioException("Volume em ml é obrigatório para bebidas.")
            ),
            "sobremesa" => _servico.AdicionarSobremesa(
                request.Nome,
                request.PrecoBase,
                request.Gelada ?? throw new DominioException("A indicação se a sobremesa é gelada é obrigatória.")
            ),
            _ => throw new DominioException($"Tipo de item inválido: '{request.Tipo}'. Opções permitidas: prato, bebida, sobremesa.")
        };

        var response = ItemCardapioResponse.DeDominio(novoItem);
        return CreatedAtAction(nameof(ListarCardapio), new { codigo = novoItem.Codigo }, response);
    }

    /// <summary>
    /// Menu 2: Listar cardápio completo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ItemCardapioResponse>), StatusCodes.Status200OK)]
    public IActionResult ListarCardapio()
    {
        var itens = _servico.Listar();
        var response = itens.Select(ItemCardapioResponse.DeDominio).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Menu 3: Buscar item por nome (correspondência parcial, case-insensitive).
    /// </summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IReadOnlyList<ItemCardapioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult BuscarPorNome([FromQuery] string? nome)
    {
        var itens = _servico.BuscarPorNome(nome);
        var response = itens.Select(ItemCardapioResponse.DeDominio).ToList();
        return Ok(response);
    }
}
