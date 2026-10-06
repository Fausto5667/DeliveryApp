namespace DeliveryApi.Servicos;

public record NovoItemCardapio(
    Dominio.Enums.CategoriaItem Categoria,
    string Nome,
    decimal PrecoBase,
    int? TempoPreparo,
    int? VolumeMl,
    bool? Gelada);

public record ItemSolicitado(int CodigoItem, int Quantidade);

public record NovoPedido(
    string TelefoneCliente,
    int EntregadorId,
    decimal DistanciaKm,
    IReadOnlyList<ItemSolicitado> Itens);

public record SimulacaoPedido(
    decimal Subtotal,
    decimal Frete,
    decimal Total,
    int TempoPreparoMinutos);
