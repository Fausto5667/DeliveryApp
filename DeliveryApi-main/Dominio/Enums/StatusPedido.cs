namespace DeliveryApi.Dominio.Enums;

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
    public static StatusPedido? ObterProximo(this StatusPedido status) => status switch
    {
        StatusPedido.Recebido => StatusPedido.EmPreparo,
        StatusPedido.EmPreparo => StatusPedido.Pronto,
        StatusPedido.Pronto => StatusPedido.EmRota,
        StatusPedido.EmRota => StatusPedido.Entregue,
        _ => null
    };
}
