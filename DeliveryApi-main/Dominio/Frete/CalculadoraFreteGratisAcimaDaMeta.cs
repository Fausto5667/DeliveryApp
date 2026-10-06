using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dominio.Frete;

public class CalculadoraFreteGratisAcimaDaMeta : ICalculadoraFrete
{
    private readonly ICalculadoraFrete _calculadoraInterna;
    private readonly decimal _meta;

    public CalculadoraFreteGratisAcimaDaMeta(ICalculadoraFrete calculadoraInterna, decimal meta = RegrasDeNegocio.MetaFreteGratis)
    {
        _calculadoraInterna = calculadoraInterna ?? throw new ArgumentNullException(nameof(calculadoraInterna));
        _meta = meta;
    }

    public decimal Calcular(TipoVeiculo veiculo, decimal distanciaKm, decimal valorPedido) =>
        valorPedido > _meta
            ? 0m
            : _calculadoraInterna.Calcular(veiculo, distanciaKm, valorPedido);
}
