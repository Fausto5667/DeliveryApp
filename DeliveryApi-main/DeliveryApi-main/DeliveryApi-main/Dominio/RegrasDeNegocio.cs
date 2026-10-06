using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio;

/// <summary>
/// Centraliza as constantes numéricas e políticas de precificação de frete do domínio.
/// Evita 'números mágicos' espalhados pelo código e encapsula as regras de tarifação.
/// </summary>
public static class RegrasDeNegocio
{
    /// <summary>
    /// Pedidos cujo subtotal atinja ou supere este valor recebem frete gratuito automaticamente.
    /// </summary>
    public const decimal MetaFreteGratis = 80.00m;

    /// <summary>
    /// Taxa por quilômetro para entrega por Bicicleta: R$ 1,00/km.
    /// </summary>
    public const decimal TaxaKmBicicleta = 1.00m;

    /// <summary>
    /// Taxa por quilômetro para entrega por Moto: R$ 1,50/km.
    /// </summary>
    public const decimal TaxaKmMoto = 1.50m;

    /// <summary>
    /// Taxa por quilômetro para entrega por Carro: R$ 2,00/km.
    /// </summary>
    public const decimal TaxaKmCarro = 2.00m;

    /// <summary>
    /// Retorna a taxa por quilômetro com base na modalidade de veículo do entregador.
    /// </summary>
    public static decimal TaxaPorKm(TipoVeiculo veiculo) => veiculo switch
    {
        TipoVeiculo.Bicicleta => TaxaKmBicicleta,
        TipoVeiculo.Moto => TaxaKmMoto,
        TipoVeiculo.Carro => TaxaKmCarro,
        _ => throw new DominioException($"Modalidade de veículo inválida: {veiculo}")
    };
}
