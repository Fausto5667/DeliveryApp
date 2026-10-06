using DeliveryApi.Dominio.Enums;
using DeliveryApi.Servicos;

namespace DeliveryApi.Dtos;

public record ItemMaisVendidoDto(
    int Codigo,
    string Nome,
    CategoriaItem Categoria,
    int QuantidadeVendida,
    decimal TotalVendido
)
{
    public decimal TotalFaturado => TotalVendido;
}

public record RelatorioGerencialResponse(
    IReadOnlyList<ItemMaisVendidoDto> ItensMaisVendidos,
    double? TempoMedioEntregaMinutos,
    decimal FaturamentoTotal,
    int TotalPedidosEntregues,
    int TotalPedidos,
    string? MensagemTempoMedio = null
)
{
    public int TotalPedidosGeral => TotalPedidos;

    public static RelatorioGerencialResponse DeDominio(RelatorioGerencial relatorio) => new(
        relatorio.ItensMaisVendidos
            .Select(i => new ItemMaisVendidoDto(i.Codigo, i.Nome, i.Categoria, i.QuantidadeVendida, i.TotalVendido))
            .ToList()
            .AsReadOnly(),
        relatorio.TempoMedioEntregaMinutos,
        relatorio.FaturamentoTotal,
        relatorio.TotalPedidosEntregues,
        relatorio.TotalPedidos,
        relatorio.TempoMedioEntregaMinutos.HasValue
            ? $"{relatorio.TempoMedioEntregaMinutos.Value:N1} minutos"
            : "Nenhum pedido entregue ainda"
    );
}
