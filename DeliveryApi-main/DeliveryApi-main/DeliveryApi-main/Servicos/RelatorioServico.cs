using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

/// <summary>
/// Modelo de dados representativo para itens mais vendidos no relatório gerencial.
/// </summary>
public record ItemMaisVendidoRelatorio(
    int Codigo,
    string Nome,
    string Categoria,
    int QuantidadeVendida,
    decimal TotalFaturado
);

/// <summary>
/// Modelo de dados representativo para o relatório gerencial completo.
/// </summary>
public record RelatorioGerencial(
    IReadOnlyList<ItemMaisVendidoRelatorio> ItensMaisVendidos,
    double TempoMedioEntregaMinutos,
    string MensagemTempoMedio,
    decimal FaturamentoTotal,
    int TotalPedidosEntregues,
    int TotalPedidosGeral
);

/// <summary>
/// Camada de Serviço para emissão de métricas e relatórios gerenciais consolidados do negócio.
/// </summary>
public class RelatorioServico
{
    private readonly IRepositorio<Pedido> _repositorioPedido;

    public RelatorioServico(IRepositorio<Pedido> repositorioPedido)
    {
        _repositorioPedido = repositorioPedido;
    }

    public RelatorioGerencial Gerar()
    {
        var todosPedidos = _repositorioPedido.ListarTodos();
        var pedidosEntregues = todosPedidos.Where(p => p.Status == StatusPedido.Entregue).ToList();

        // 1. Itens mais vendidos (Top 5 a partir de todos os pedidos)
        var itensMaisVendidos = todosPedidos
            .SelectMany(p => p.Itens)
            .GroupBy(i => new { i.Item.Codigo, i.Item.Nome, i.Item.Categoria })
            .Select(g => new ItemMaisVendidoRelatorio(
                g.Key.Codigo,
                g.Key.Nome,
                g.Key.Categoria,
                g.Sum(x => x.Quantidade),
                g.Sum(x => x.Subtotal)
            ))
            .OrderByDescending(x => x.QuantidadeVendida)
            .Take(5)
            .ToList()
            .AsReadOnly();

        // 2. Tempo médio de entrega em minutos
        double tempoMedioMinutos = 0.0;
        string mensagemTempoMedio;

        var entreguesComData = pedidosEntregues
            .Where(p => p.EntregueEm.HasValue)
            .ToList();

        if (entreguesComData.Any())
        {
            tempoMedioMinutos = entreguesComData
                .Average(p => (p.EntregueEm!.Value - p.CriadoEm).TotalMinutes);
            tempoMedioMinutos = Math.Round(tempoMedioMinutos, 2);
            mensagemTempoMedio = $"{tempoMedioMinutos} minutos";
        }
        else
        {
            mensagemTempoMedio = "Nenhum pedido entregue até o momento.";
        }

        // 3. Faturamento total dos pedidos entregues
        decimal faturamentoTotal = pedidosEntregues.Sum(p => p.Total);

        return new RelatorioGerencial(
            itensMaisVendidos,
            tempoMedioMinutos,
            mensagemTempoMedio,
            faturamentoTotal,
            pedidosEntregues.Count,
            todosPedidos.Count
        );
    }
}
