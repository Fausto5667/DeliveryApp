namespace DeliveryApi.Dominio.Enums;

/// <summary>
/// Ciclo de vida do pedido. A ordem dos valores é a ordem obrigatória das transições:
/// Recebido, EmPreparo, Pronto, EmRota e Entregue.
/// </summary>
public enum StatusPedido
{
    Recebido = 1,
    EmPreparo = 2,
    Pronto = 3,
    EmRota = 4,
    Entregue = 5
}

public static class StatusPedidoExtensoes
{
    /// <summary>Próximo estado da sequência, ou null quando o pedido já está no estado final.</summary>
    public static StatusPedido? ObterProximo(this StatusPedido status) => status switch
    {
        StatusPedido.Recebido => StatusPedido.EmPreparo,
        StatusPedido.EmPreparo => StatusPedido.Pronto,
        StatusPedido.Pronto => StatusPedido.EmRota,
        StatusPedido.EmRota => StatusPedido.Entregue,
        _ => null
    };
}
