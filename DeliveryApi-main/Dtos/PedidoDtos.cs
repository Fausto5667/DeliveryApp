using System.ComponentModel.DataAnnotations;
using DeliveryApi.Dominio;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;
using DeliveryApi.Servicos;

namespace DeliveryApi.Dtos;

public record ItemPedidoRequest(
    [Range(1, int.MaxValue, ErrorMessage = "O código do item deve ser válido.")]
    int CodigoItem,

    [Range(1, RegrasDeNegocio.QuantidadeMaximaPorItem, ErrorMessage = "A quantidade deve ser entre 1 e 100 unidades.")]
    int Quantidade
);

public record CriarPedidoRequest(
    [Required(ErrorMessage = "O telefone do cliente é obrigatório.")]
    string TelefoneCliente,

    [Range(1, int.MaxValue, ErrorMessage = "O identificador do entregador deve ser válido.")]
    int EntregadorId,

    [Range(0.1, 500.0, ErrorMessage = "A distância estimada deve estar entre 0,1 e 500 km.")]
    decimal DistanciaKm,

    [Required(ErrorMessage = "A lista de itens do pedido é obrigatória.")]
    [MinLength(1, ErrorMessage = "O pedido deve conter ao menos um item.")]
    List<ItemPedidoRequest> Itens
) : IValidatableObject
{
    /// <summary>A validação por atributos não percorre os elementos da lista, então um item nulo passaria.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Itens is not null && Itens.Any(i => i is null))
            yield return new ValidationResult("A lista de itens não pode conter elementos vazios.", new[] { nameof(Itens) });
    }

    public NovoPedido ParaComando() =>
        new(TelefoneCliente, EntregadorId, DistanciaKm,
            Itens.Select(i => new ItemSolicitado(i.CodigoItem, i.Quantidade)).ToList());
}

public record ItemPedidoResponse(
    int CodigoItem,
    string NomeItem,
    CategoriaItem Categoria,
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

public record PedidoResponse(
    int Id,
    StatusPedido Status,
    StatusPedido? ProximoStatus,
    ClienteResponse Cliente,
    EntregadorResponse Entregador,
    decimal DistanciaKm,
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
        pedido.ProximoStatus,
        ClienteResponse.DeDominio(pedido.Cliente),
        EntregadorResponse.DeDominio(pedido.Entregador),
        pedido.DistanciaKm,
        pedido.Itens.Select(ItemPedidoResponse.DeDominio).ToList().AsReadOnly(),
        pedido.Subtotal,
        pedido.Frete,
        pedido.Total,
        pedido.TempoPreparoMinutos,
        pedido.CriadoEm,
        pedido.EntregueEm
    );
}

public record SimulacaoPedidoResponse(
    decimal Subtotal,
    decimal Frete,
    decimal Total,
    bool FreteGratis,
    int TempoPreparoMinutos
)
{
    public static SimulacaoPedidoResponse DeDominio(SimulacaoPedido simulacao) => new(
        simulacao.Subtotal,
        simulacao.Frete,
        simulacao.Total,
        simulacao.Frete == 0m,
        simulacao.TempoPreparoMinutos
    );
}
