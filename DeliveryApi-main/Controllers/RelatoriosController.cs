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

    [HttpGet]
    [HttpGet("gerencial")]
    [ProducesResponseType(typeof(RelatorioGerencialResponse), StatusCodes.Status200OK)]
    public IActionResult EmitirRelatorioGerencial() =>
        Ok(RelatorioGerencialResponse.DeDominio(_servico.Gerar()));
}
