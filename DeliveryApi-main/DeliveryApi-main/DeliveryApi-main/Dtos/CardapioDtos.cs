using System.ComponentModel.DataAnnotations;
using DeliveryApi.Dominio.Entidades;

namespace DeliveryApi.Dtos;

/// <summary>
/// DTO de requisição para cadastro de novo item no cardápio.
/// </summary>
public record CriarItemRequest(
    [Required(ErrorMessage = "O tipo do item é obrigatório ('prato', 'bebida' ou 'sobremesa').")]
    string Tipo,

    [Required(ErrorMessage = "O nome do item é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve conter entre 2 e 100 caracteres.")]
    string Nome,

    [Range(0.01, 10000.0, ErrorMessage = "O preço base deve ser maior que zero.")]
    decimal PrecoBase,

    int? TempoPreparo,
    int? VolumeMl,
    bool? Gelada
);

/// <summary>
/// DTO de resposta expondo informações do item de cardápio sem vazar a entidade do domínio.
/// </summary>
public record ItemCardapioResponse(
    int Codigo,
    string Nome,
    decimal PrecoBase,
    string Categoria,
    string Detalhes,
    int TempoPreparoMinutos
)
{
    /// <summary>
    /// CONCEITO POO - POLIMORFISMO:
    /// Converte qualquer subtipo de ItemCardapio para a resposta invocando Categoria,
    /// ObterDetalhes() e TempoPreparoMinutos dinamicamente sem if/switch de tipo!
    /// </summary>
    public static ItemCardapioResponse DeDominio(ItemCardapio item) => new(
        item.Codigo,
        item.Nome,
        item.PrecoBase,
        item.Categoria,
        item.ObterDetalhes(),
        item.TempoPreparoMinutos
    );
}
