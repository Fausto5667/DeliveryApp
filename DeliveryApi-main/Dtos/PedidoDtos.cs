using System.ComponentModel.DataAnnotations;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dtos;

/// <summary>
/// Linha de item na requisição de abertura de pedido.
/// </summary>
public record ItemPedidoRequest(
    [Range(1, int.MaxValue, ErrorMessage = "O código do item deve ser válido.")]
    int CodigoItem,

    [Range(1, 100, ErrorMessage = "A quantidade deve ser entre 1 e 100 unidades.")]
    int Quantidade
);

/// <summary>
/// DTO de requisição para montagem de novo pedido.
/// </summary>
public record CriarPedidoRequest(
    [Required(ErrorMessage = "O telefone do cliente é obrigatório.")]
    string TelefoneCliente,

    [Range(1, int.MaxValue, ErrorMessage = "O identificador do entregador deve ser válido.")]
    int EntregadorId,

    [Range(0.1, 500.0, ErrorMessage = "A distância estimada em km deve ser maior que zero.")]
    decimal DistanciaKm,

    [Required(ErrorMessage = "A lista de itens do pedido é obrigatória.")]
    [MinLength(1, ErrorMessage = "O pedido deve conter ao menos um item.")]
    List<ItemPedidoRequest> Itens
);

/// <summary>
/// DTO de saída detalhando uma linha de item do pedido.
/// </summary>
public record ItemPedidoResponse(
    int CodigoItem,
    string NomeItem,
    string Categoria,
    decimal PrecoUnitario,
    int Quantidade,
    decimal Subtotal
)
{
    public static ItemPedidoResponse DeDominio(ItemPedido linha) => new(
        linha.Item.Codigo,
        linha.Item.Nome,
        linha.Item.Categoria,
        linha.Item.PrecoBase,
        linha.Quantidade,
        linha.Subtotal
    );
}

/// <summary>
/// DTO de saída expondo todas as informações agregadas e calculadas do pedido.
/// </summary>
public record PedidoResponse(
    int Id,
    StatusPedido Status,
    ClienteResponse Cliente,
    EntregadorResponse Entregador,
    IReadOnlyList<ItemPedidoResponse> Itens,
    decimal Subtotal,
    decimal Frete,
    decimal Total,
    int TempoPreparoMinutos,
    DateTime CriadoEm,
    DateTime? EntregueEm
)
{
    public static PedidoResponse DeDominio(Pedido pedido) => new(
        pedido.Id,
        pedido.Status,
        ClienteResponse.DeDominio(pedido.Cliente),
        EntregadorResponse.DeDominio(pedido.Entregador),
        pedido.Itens.Select(ItemPedidoResponse.DeDominio).ToList().AsReadOnly(),
        pedido.Subtotal,
        pedido.Frete,
        pedido.Total,
        pedido.TempoPreparoMinutos,
        pedido.CriadoEm,
        pedido.EntregueEm
    );
}
