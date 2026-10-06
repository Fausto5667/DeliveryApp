namespace DeliveryApi.Servicos;

/// <summary>Dados necessários para cadastrar um item do cardápio, independentes do formato da API.</summary>
public record NovoItemCardapio(
    Dominio.Enums.CategoriaItem Categoria,
    string Nome,
    decimal PrecoBase,
    int? TempoPreparo,
    int? VolumeMl,
    bool? Gelada);

public record ItemSolicitado(int CodigoItem, int Quantidade);

/// <summary>Dados necessários para montar (ou simular) um pedido.</summary>
public record NovoPedido(
    string TelefoneCliente,
    int EntregadorId,
    decimal DistanciaKm,
    IReadOnlyList<ItemSolicitado> Itens);

/// <summary>Resultado de uma simulação: valores que o pedido teria, sem criá-lo nem alocar o entregador.</summary>
public record SimulacaoPedido(
    decimal Subtotal,
    decimal Frete,
    decimal Total,
    int TempoPreparoMinutos);
