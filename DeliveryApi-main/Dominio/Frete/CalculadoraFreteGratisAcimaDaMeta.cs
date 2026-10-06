using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dominio.Frete;

/// <summary>
/// Regra de frete grátis: se o valor do pedido supera a meta o frete é zero; caso contrário delega
/// ao cálculo interno. Agrega outra ICalculadoraFrete recebida de fora (agregação): a calculadora interna
/// existe e funciona sozinha, e esta classe apenas decora o seu resultado.
/// </summary>
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
