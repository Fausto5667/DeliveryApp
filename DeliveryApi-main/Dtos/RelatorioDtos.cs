using DeliveryApi.Servicos;

namespace DeliveryApi.Dtos;

/// <summary>
/// DTO de item mais vendido no relatório gerencial.
/// </summary>
public record ItemMaisVendidoDto(
    int Codigo,
    string Nome,
    string Categoria,
    int QuantidadeVendida,
    decimal TotalFaturado
);

/// <summary>
/// DTO de saída para o relatório gerencial completo.
/// </summary>
public record RelatorioGerencialResponse(
    IReadOnlyList<ItemMaisVendidoDto> ItensMaisVendidos,
    double TempoMedioEntregaMinutos,
    string MensagemTempoMedio,
    decimal FaturamentoTotal,
    int TotalPedidosEntregues,
    int TotalPedidosGeral
)
{
    public static RelatorioGerencialResponse DeDominio(RelatorioGerencial relatorio) => new(
        relatorio.ItensMaisVendidos.Select(i => new ItemMaisVendidoDto(
            i.Codigo,
            i.Nome,
            i.Categoria,
            i.QuantidadeVendida,
            i.TotalFaturado
        )).ToList().AsReadOnly(),
        relatorio.TempoMedioEntregaMinutos,
        relatorio.MensagemTempoMedio,
        relatorio.FaturamentoTotal,
        relatorio.TotalPedidosEntregues,
        relatorio.TotalPedidosGeral
    );
}
