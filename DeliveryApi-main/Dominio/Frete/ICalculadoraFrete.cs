using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dominio.Frete;

public interface ICalculadoraFrete
{
    decimal Calcular(TipoVeiculo veiculo, decimal distanciaKm, decimal valorPedido);
}
