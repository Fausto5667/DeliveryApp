using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dominio.Frete;

public class CalculadoraFretePorVeiculo : ICalculadoraFrete
{
    public decimal Calcular(TipoVeiculo veiculo, decimal distanciaKm, decimal valorPedido) =>
        Math.Round(RegrasDeNegocio.TaxaPorKm(veiculo) * distanciaKm, 2, MidpointRounding.AwayFromZero);
}
