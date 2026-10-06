using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly RelatorioServico _servico;

    public RelatoriosController(RelatorioServico servico)
    {
        _servico = servico;
    }

    /// <summary>
    /// Menu 9: Emitir relatório gerencial (itens mais vendidos, tempo médio de entrega e faturamento total).
    /// </summary>
    [HttpGet("gerencial")]
    [ProducesResponseType(typeof(RelatorioGerencialResponse), StatusCodes.Status200OK)]
    public IActionResult EmitirRelatorioGerencial()
    {
        var relatorio = _servico.Gerar();
        var response = RelatorioGerencialResponse.DeDominio(relatorio);
        return Ok(response);
    }
}
