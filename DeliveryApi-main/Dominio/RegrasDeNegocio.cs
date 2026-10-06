using DeliveryApi.Dominio.Enums;
using DeliveryApi.Dominio.Excecoes;

namespace DeliveryApi.Dominio;

public static class RegrasDeNegocio
{
    public const decimal MetaFreteGratis = 80.00m;

    public const decimal TaxaKmBicicleta = 1.00m;
    public const decimal TaxaKmMoto = 1.50m;
    public const decimal TaxaKmCarro = 2.00m;

    public const decimal PrecoMaximoItem = 10_000m;
    public const int TempoPreparoMaximoMinutos = 600;
    public const int VolumeMaximoMl = 10_000;
    public const int QuantidadeMaximaPorItem = 100;
    public const decimal DistanciaMaximaKm = 500m;

    public const int TelefoneMinimoDigitos = 8;
    public const int TelefoneMaximoDigitos = 15;

    public static decimal TaxaPorKm(TipoVeiculo veiculo) => veiculo switch
    {
        TipoVeiculo.Bicicleta => TaxaKmBicicleta,
        TipoVeiculo.Moto => TaxaKmMoto,
        TipoVeiculo.Carro => TaxaKmCarro,
        _ => throw new DominioException($"Modalidade de veículo inválida: {veiculo}")
    };
}
