using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Repositorios;

namespace DeliveryApi.Servicos;

public record ItemMaisVendido(
    int Codigo,
    string Nome,
    CategoriaItem Categoria,
    int QuantidadeVendida,
    decimal TotalVendido);

public record RelatorioGerencial(
    IReadOnlyList<ItemMaisVendido> ItensMaisVendidos,
    double? TempoMedioEntregaMinutos,
    decimal FaturamentoTotal,
    int TotalPedidosEntregues,
    int TotalPedidos);

public class RelatorioServico
{
    private const int QuantidadeNoRanking = 5;

    private readonly IRepositorio<Pedido, int> _repositorioPedido;

    public RelatorioServico(IRepositorio<Pedido, int> repositorioPedido)
    {
        _repositorioPedido = repositorioPedido;
    }

    public RelatorioGerencial Gerar()
    {
        var pedidos = _repositorioPedido.ListarTodos();
        var entregues = pedidos.Where(p => p.Status == StatusPedido.Entregue && p.EntregueEm.HasValue).ToList();

        var maisVendidos = pedidos
            .SelectMany(p => p.Itens)
            .GroupBy(i => i.Item.Codigo)
            .Select(g => new ItemMaisVendido(
                g.Key,
                g.First().Item.Nome,
                g.First().Item.Categoria,
                g.Sum(i => i.Quantidade),
                g.Sum(i => i.Subtotal)))
            .OrderByDescending(i => i.QuantidadeVendida)
            .ThenBy(i => i.Nome, StringComparer.CurrentCultureIgnoreCase)
            .Take(QuantidadeNoRanking)
            .ToList()
            .AsReadOnly();

        double? tempoMedio = entregues.Count > 0
            ? Math.Round(entregues.Average(p => (p.EntregueEm!.Value - p.CriadoEm).TotalMinutes), 2)
            : null;

        return new RelatorioGerencial(
            maisVendidos,
            tempoMedio,
            entregues.Sum(p => p.Total),
            entregues.Count,
            pedidos.Count);
    }
}
