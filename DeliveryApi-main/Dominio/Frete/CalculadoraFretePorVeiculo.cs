using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dominio.Frete;

/// <summary>Frete = taxa por km do veículo x distância, arredondado em centavos.</summary>
public class CalculadoraFretePorVeiculo : ICalculadoraFrete
{
    public decimal Calcular(TipoVeiculo veiculo, decimal distanciaKm, decimal valorPedido) =>
        Math.Round(RegrasDeNegocio.TaxaPorKm(veiculo) * distanciaKm, 2, MidpointRounding.AwayFromZero);
}
