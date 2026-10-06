using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dominio.Frete;

/// <summary>
/// Contrato de cálculo do frete. Quem precisa de um frete (o Pedido) depende apenas deste contrato e não
/// sabe como a taxa é composta, o que permite combinar regras (tarifa por veículo + gratuidade) e
/// trocá-las sem alterar o Pedido.
/// </summary>
public interface ICalculadoraFrete
{
    decimal Calcular(TipoVeiculo veiculo, decimal distanciaKm, decimal valorPedido);
}
