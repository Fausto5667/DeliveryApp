using System.ComponentModel.DataAnnotations;
using DeliveryApi.Dominio.Entidades;
using DeliveryApi.Dominio.Enums;

namespace DeliveryApi.Dtos;

/// <summary>
/// DTO de requisição para cadastro de entregador.
/// </summary>
public record CriarEntregadorRequest(
    [Required(ErrorMessage = "O nome do entregador é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve conter entre 2 e 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "A modalidade do veículo é obrigatória ('Moto', 'Bicicleta' ou 'Carro').")]
    TipoVeiculo Veiculo
);

/// <summary>
/// DTO de resposta para exibição de entregador da frota.
/// </summary>
public record EntregadorResponse(
    int Id,
    string Nome,
    TipoVeiculo Veiculo,
    bool Disponivel
)
{
    public static EntregadorResponse DeDominio(Entregador entregador) => new(
        entregador.Id,
        entregador.Nome,
        entregador.Veiculo,
        entregador.Disponivel
    );
}
