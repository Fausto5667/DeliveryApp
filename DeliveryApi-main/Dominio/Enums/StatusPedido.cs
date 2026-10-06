namespace DeliveryApi.Dominio.Enums;

/// <summary>
/// Representa a máquina de estados do ciclo de vida do pedido.
/// Percorre obrigatoriamente a sequência: Recebido -> EmPreparo -> Pronto -> EmRota -> Entregue.
/// </summary>
public enum StatusPedido
{
    Recebido = 1,
    EmPreparo = 2,
    Pronto = 3,
    EmRota = 4,
    Entregue = 5
}
