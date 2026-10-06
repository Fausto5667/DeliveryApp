using Microsoft.AspNetCore.Mvc;
using DeliveryApi.Dtos;
using DeliveryApi.Servicos;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/relatorios")]
public class RelatoriosController : ControllerBase
{
    private readonly RelatorioServico _servico;

    public RelatoriosController(RelatorioServico servico)
    {
        _servico = servico;
    }

    /// <summary>Menu 9: relatório gerencial (itens mais vendidos, tempo médio de entrega e faturamento).</summary>
    [HttpGet]
    [HttpGet("gerencial")]
    [ProducesResponseType(typeof(RelatorioGerencialResponse), StatusCodes.Status200OK)]
    public IActionResult EmitirRelatorioGerencial() =>
        Ok(RelatorioGerencialResponse.DeDominio(_servico.Gerar()));
}
